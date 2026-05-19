namespace LoupedeckMSFSG1000.Sim;

using System.Linq.Expressions;
using System.Reflection;
using LoupedeckMSFSG1000;

public sealed class WaSimReflectionClient : ISimClient
{
    private const UInt32 ClientId = 0x4C4F4749;
    private readonly Object _syncRoot = new();
    private readonly Dictionary<UInt32, SimSubscription> _subscriptions = new();
    private Object? _client;
    private Type? _clientType;
    private Type? _dataRequestType;
    private Assembly? _assembly;
    private Boolean _isConnected;
    private Boolean _callbackAttached;
    private UInt32 _nextRequestId = 1;

    public event EventHandler<SimValueChangedEventArgs>? ValueChanged;

    public Boolean IsConnected => _isConnected;

    public static Boolean CanResolveClientLibrary() => ResolveAssemblyPath() is not null;

    public static String GetResolutionDiagnostics()
    {
        var candidates = GetAssemblyPathCandidates();
        return $"PluginAssembly='{GetPluginAssemblyDirectory()}'; AppContext='{AppContext.BaseDirectory}'; CurrentDirectory='{Environment.CurrentDirectory}'; Candidates={String.Join(" | ", candidates.Select(path => $"{path}:{File.Exists(path)}"))}";
    }

    public Task ConnectAsync(CancellationToken cancellationToken) =>
        Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_syncRoot)
            {
                if (_isConnected)
                {
                    return;
                }

                this.EnsureClientCreated();

                var simulatorResult = InvokeFirst(this.RequireClient(), ["connectSimulator", "ConnectSimulator"], new Object?[] { null });
                EnsureOk(simulatorResult, "connectSimulator");

                var serverResult = InvokeFirst(this.RequireClient(), ["connectServer", "ConnectServer"], new Object?[] { null });
                EnsureOk(serverResult, "connectServer");

                this.AttachDataReceivedEvent();
                _isConnected = true;
                PluginLog.Info("Connected to WASimCommander server.");
            }
        }, cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken) =>
        Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_syncRoot)
            {
                if (_client is not null)
                {
                    TryInvoke(_client, "disconnectSimulator", "DisconnectSimulator", "Disconnect");
                }

                _subscriptions.Clear();
                _isConnected = false;
                _callbackAttached = false;
            }
        }, cancellationToken);

    public async Task ExecuteCalculatorCodeAsync(String calculatorCode, CancellationToken cancellationToken)
    {
        await this.ConnectAsync(cancellationToken).ConfigureAwait(false);
        await Task.Run(() =>
        {
            lock (_syncRoot)
            {
                var result = InvokeFirst(this.RequireClient(), ["executeCalculatorCode", "ExecuteCalculatorCode"], calculatorCode);
                PluginLog.Info($"WASim calculator command sent: {calculatorCode} ({result})");
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task SubscribeAsync(SimSubscription subscription, CancellationToken cancellationToken)
    {
        await this.ConnectAsync(cancellationToken).ConfigureAwait(false);
        await Task.Run(() =>
        {
            lock (_syncRoot)
            {
                var requestId = _nextRequestId++;
                var request = this.CreateCalculatorDataRequest(requestId, subscription);
                var result = InvokeFirst(this.RequireClient(), ["saveDataRequest", "SaveDataRequest"], request);
                EnsureOk(result, "saveDataRequest");
                _subscriptions[requestId] = subscription;
                PluginLog.Info($"WASim subscription added: {subscription.Id} -> {subscription.CalculatorCode}");
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        try
        {
            this.DisconnectAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "WASim disconnect failed.");
        }
    }

    private void EnsureClientCreated()
    {
        if (_client is not null)
        {
            return;
        }

        var assemblyPath = ResolveAssemblyPath()
            ?? throw new FileNotFoundException("WASimCommander.WASimClient.dll was not found.");
        PluginLog.Info($"Loading WASimCommander client from '{assemblyPath}'.");
        _assembly = Assembly.LoadFrom(assemblyPath);
        _clientType = FindType(_assembly, "WASimClient") ?? throw new MissingMemberException("WASimClient type not found.");
        _dataRequestType = FindType(_assembly, "DataRequest") ?? throw new MissingMemberException("DataRequest type not found.");
        _client = Activator.CreateInstance(_clientType, ClientId) ?? throw new InvalidOperationException("Could not create WASimClient.");
    }

    private Object CreateCalculatorDataRequest(UInt32 requestId, SimSubscription subscription)
    {
        var assembly = _assembly ?? throw new InvalidOperationException("WASim assembly is not loaded.");
        var dataRequestType = _dataRequestType ?? throw new InvalidOperationException("WASim DataRequest type is not loaded.");
        var calcResultType = assembly.GetType("WASimCommander.CLI.Enums.CalcResultType")
            ?? throw new MissingMemberException("CalcResultType enum not found.");
        var updatePeriodType = assembly.GetType("WASimCommander.CLI.Enums.UpdatePeriod")
            ?? throw new MissingMemberException("UpdatePeriod enum not found.");

        var constructor = dataRequestType.GetConstructor([
            typeof(UInt32),
            calcResultType,
            typeof(String),
            updatePeriodType,
            typeof(UInt32),
            typeof(Single),
        ]) ?? throw new MissingMethodException(dataRequestType.FullName, "DataRequest calculator constructor");

        return constructor.Invoke([
            requestId,
            Enum.Parse(calcResultType, ToCalcResultName(subscription.ValueKind)),
            subscription.CalculatorCode,
            Enum.Parse(updatePeriodType, "Millisecond"),
            Convert.ToUInt32(Math.Max(50, subscription.Interval.TotalMilliseconds)),
            0.0f,
        ]);
    }

    private void AttachDataReceivedEvent()
    {
        if (_callbackAttached)
        {
            return;
        }

        var client = this.RequireClient();
        var eventInfo = (_clientType ?? client.GetType()).GetEvent("OnDataReceived") ?? client.GetType().GetEvent("DataReceived");
        var handlerType = eventInfo?.EventHandlerType;
        var invokeMethod = handlerType?.GetMethod("Invoke");
        if (eventInfo is null || handlerType is null || invokeMethod is null)
        {
            PluginLog.Warning("WASim data callback event not found.");
            return;
        }

        var parameters = invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        var handlerMethod = typeof(WaSimReflectionClient).GetMethod(nameof(this.OnRawDataReceived), BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(WaSimReflectionClient), nameof(this.OnRawDataReceived));
        var body = Expression.Call(Expression.Constant(this), handlerMethod, Expression.Convert(parameters[0], typeof(Object)));
        var handler = Expression.Lambda(handlerType, body, parameters).Compile();
        eventInfo.AddEventHandler(client, handler);
        _callbackAttached = true;
    }

    private void OnRawDataReceived(Object record)
    {
        try
        {
            var requestId = Convert.ToUInt32(ReadMember(record, "requestId") ?? 0);
            if (!_subscriptions.TryGetValue(requestId, out var subscription))
            {
                return;
            }

            var value = ConvertRecordValue(record, subscription.ValueKind);
            this.ValueChanged?.Invoke(this, new SimValueChangedEventArgs(subscription.Id, value, DateTimeOffset.UtcNow));
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "WASim data callback parsing failed.");
        }
    }

    private Object RequireClient() => _client ?? throw new InvalidOperationException("WASim client is not initialized.");

    private static String ToCalcResultName(SimValueKind valueKind) =>
        valueKind == SimValueKind.String ? "String" : "Double";

    private static Double ConvertRecordValue(Object record, SimValueKind valueKind)
    {
        if (valueKind == SimValueKind.Boolean)
        {
            return ConvertRecordValue(record, SimValueKind.Double) > 0.5 ? 1.0 : 0.0;
        }

        var bytes = ReadMember(record, "data") as Byte[];
        if (bytes is not null && bytes.Length >= sizeof(Double))
        {
            return BitConverter.ToDouble(bytes, 0);
        }

        return 0.0;
    }

    private static Object? ReadMember(Object target, String name)
    {
        var type = target.GetType();
        return type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target)
            ?? type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target);
    }

    private static String? ResolveAssemblyPath()
    {
        return GetAssemblyPathCandidates().FirstOrDefault(File.Exists);
    }

    private static String[] GetAssemblyPathCandidates() =>
    [
        Environment.GetEnvironmentVariable("WASIMCOMMANDER_CLIENT_DLL") ?? String.Empty,
        Path.Combine(GetPluginAssemblyDirectory(), "WASimCommander.WASimClient.dll"),
        Path.Combine(GetPluginAssemblyDirectory(), "bin", "WASimCommander.WASimClient.dll"),
        Path.Combine(GetInstalledPluginBinDirectory(), "WASimCommander.WASimClient.dll"),
        Path.Combine(AppContext.BaseDirectory, "WASimCommander.WASimClient.dll"),
        Path.Combine(AppContext.BaseDirectory, "bin", "WASimCommander.WASimClient.dll"),
        Path.Combine(Environment.CurrentDirectory, "WASimCommander.WASimClient.dll"),
        Path.Combine(Environment.CurrentDirectory, "bin", "WASimCommander.WASimClient.dll"),
        FindFromRepositoryExternalFolder() ?? String.Empty,
    ];

    private static String? FindFromRepositoryExternalFolder()
    {
        var directory = new DirectoryInfo(GetPluginAssemblyDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".external", "WASimCommander", "SDK", "lib", "managed", "net8", "WASimCommander.WASimClient.dll");
            if (File.Exists(candidate))
            {
                CopyRuntimeCompanionFiles(Path.GetDirectoryName(candidate)!);
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static String GetPluginAssemblyDirectory() =>
        Path.GetDirectoryName(typeof(WaSimReflectionClient).Assembly.Location) ?? AppContext.BaseDirectory;

    private static String GetInstalledPluginBinDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Logi",
            "LogiPluginService",
            "Plugins",
            "LoupedeckMSFSG1000",
            "bin");

    private static void CopyRuntimeCompanionFiles(String sourceDirectory)
    {
        foreach (var fileName in new[] { "Ijwhost.dll", "client_conf.ini" })
        {
            var source = Path.Combine(sourceDirectory, fileName);
            var destination = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(source) && !File.Exists(destination))
            {
                File.Copy(source, destination);
            }
        }
    }

    private static Type? FindType(Assembly assembly, String shortName) =>
        assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(shortName, StringComparison.OrdinalIgnoreCase));

    private static Object? InvokeFirst(Object target, IReadOnlyList<String> methodNames, params Object?[] args)
    {
        var type = target.GetType();
        foreach (var methodName in methodNames)
        {
            var method = type.GetMethods().FirstOrDefault(candidate =>
                candidate.Name == methodName &&
                candidate.GetParameters().Length == args.Length);
            if (method is null)
            {
                continue;
            }

            return method.Invoke(target, args);
        }

        throw new MissingMethodException(type.FullName, String.Join("/", methodNames));
    }

    private static Boolean TryInvoke(Object target, params String[] methodNames)
    {
        foreach (var methodName in methodNames)
        {
            var method = target.GetType().GetMethod(methodName);
            if (method is null)
            {
                continue;
            }

            method.Invoke(target, []);
            return true;
        }

        return false;
    }

    private static void EnsureOk(Object? result, String operation)
    {
        if (!String.Equals(result?.ToString(), "OK", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{operation} failed: {result}");
        }
    }
}
