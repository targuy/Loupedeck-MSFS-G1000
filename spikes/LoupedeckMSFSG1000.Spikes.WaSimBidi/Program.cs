using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

const Int32 clientId = 0x4C4F4749;
const String requestCode = "(A:AUTOPILOT MASTER, bool)";
const String toggleCode = "(>K:AP_MASTER)";

Console.WriteLine("Spike S1+S3 - WASimCommander bidirectional AP_MASTER probe");
Console.WriteLine("This spike loads WASimCommander at runtime so the repository can build without redistributing it.");

var assemblyPath = ResolveAssemblyPath(args);
if (assemblyPath is null)
{
    Console.Error.WriteLine("WASimCommander.WASimClient.dll was not found.");
    Console.Error.WriteLine("Pass its path as the first argument, set WASIMCOMMANDER_CLIENT_DLL, or copy it next to this executable.");
    return 2;
}

try
{
    var assembly = Assembly.LoadFrom(assemblyPath);
    var clientType = FindType(assembly, "WASimClient");
    var dataRequestType = FindType(assembly, "DataRequest");

    if (clientType is null || dataRequestType is null)
    {
        Console.Error.WriteLine("Could not find WASimClient/DataRequest types in the provided assembly.");
        return 3;
    }

    var client = Activator.CreateInstance(clientType, Convert.ToUInt32(clientId))
        ?? throw new InvalidOperationException("Could not create WASimClient.");

    var connectResult = InvokeFirst(client, ["connectSimulator", "ConnectSimulator"], new Object?[] { null });
    Console.WriteLine($"connectSimulator result: {connectResult}");
    if (!String.Equals(connectResult?.ToString(), "OK", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("Simulator connection failed. Start MSFS 2024, load a flight, and confirm the WASimCommander module is green in DevMode > WASM.");
        return 4;
    }

    var serverResult = InvokeFirst(client, ["connectServer", "ConnectServer"], new Object?[] { null });
    Console.WriteLine($"connectServer result: {serverResult}");
    if (!String.Equals(serverResult?.ToString(), "OK", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("WASimCommander server connection failed. Keep MSFS in a loaded flight and confirm the WASM module is active.");
        return 5;
    }

    Console.WriteLine("Connected to simulator transport. Ensure MSFS is running with the WASim module installed.");

    var request = CreateCalculatorDataRequest(assembly, dataRequestType, requestCode);

    AttachDataReceivedEvent(client, clientType);
    var saveResult = InvokeFirst(client, ["saveDataRequest", "SaveDataRequest"], request);
    Console.WriteLine($"saveDataRequest result: {saveResult}");

    Console.WriteLine($"Subscribed to: {requestCode}");
    Console.WriteLine($"Sending: {toggleCode}");
    var stopwatch = Stopwatch.StartNew();
    InvokeFirst(client, ["executeCalculatorCode", "ExecuteCalculatorCode"], toggleCode);

    Console.WriteLine("Waiting for callbacks. Press Enter to stop.");
    Console.ReadLine();
    Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");

    TryInvoke(client, "disconnectSimulator", "DisconnectSimulator", "Disconnect");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static String? ResolveAssemblyPath(String[] args)
{
    var candidates = new[]
    {
        args.FirstOrDefault(),
        Environment.GetEnvironmentVariable("WASIMCOMMANDER_CLIENT_DLL"),
        FindFromRepositoryExternalFolder(),
        Path.Combine(AppContext.BaseDirectory, "WASimCommander.WASimClient.dll"),
    };

    return candidates.FirstOrDefault(path => !String.IsNullOrWhiteSpace(path) && File.Exists(path));
}

static String? FindFromRepositoryExternalFolder()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            ".external",
            "WASimCommander",
            "SDK",
            "lib",
            "managed",
            "net8",
            "WASimCommander.WASimClient.dll");
        if (File.Exists(candidate))
        {
            CopyRuntimeCompanionFiles(Path.GetDirectoryName(candidate)!);
            return candidate;
        }

        directory = directory.Parent;
    }

    return null;
}

static void CopyRuntimeCompanionFiles(String sourceDirectory)
{
    foreach (var fileName in new[] { "Ijwhost.dll", "client_conf.ini" })
    {
        var source = Path.Combine(sourceDirectory, fileName);
        foreach (var destinationDirectory in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            var destination = Path.Combine(destinationDirectory, fileName);
            if (File.Exists(source) && !File.Exists(destination))
            {
                File.Copy(source, destination);
            }
        }
    }
}

static Type? FindType(Assembly assembly, String shortName) =>
    assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(shortName, StringComparison.OrdinalIgnoreCase));

static Object? InvokeFirst(Object target, IReadOnlyList<String> methodNames, params Object?[] args)
{
    args ??= [];
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

        var result = method.Invoke(target, args);
        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        return result;
    }

    throw new MissingMethodException(type.FullName, String.Join("/", methodNames));
}

static Object CreateCalculatorDataRequest(Assembly assembly, Type dataRequestType, String code)
{
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
    ]);

    if (constructor is null)
    {
        throw new MissingMethodException(dataRequestType.FullName, "DataRequest calculator constructor");
    }

    return constructor.Invoke([
        1u,
        Enum.Parse(calcResultType, "Double"),
        code,
        Enum.Parse(updatePeriodType, "Millisecond"),
        100u,
        0.0f,
    ]);
}

static Boolean TryInvoke(Object target, params String[] methodNames)
{
    foreach (var methodName in methodNames)
    {
        var method = target.GetType().GetMethod(methodName);
        if (method is null)
        {
            continue;
        }

        method.Invoke(target, Array.Empty<Object>());
        return true;
    }

    return false;
}

static void AttachDataReceivedEvent(Object client, Type clientType)
{
    var eventInfo = clientType.GetEvent("OnDataReceived") ?? clientType.GetEvent("DataReceived");
    if (eventInfo is null)
    {
        Console.WriteLine("No data callback event found by reflection; check the concrete WASimCommander SDK version.");
        return;
    }

    var handlerType = eventInfo.EventHandlerType;
    var invokeMethod = handlerType?.GetMethod("Invoke");
    if (handlerType is null || invokeMethod is null || invokeMethod.ReturnType != typeof(void))
    {
        Console.WriteLine($"Callback event detected but not attached: {eventInfo.Name}.");
        return;
    }

    var parameters = invokeMethod
        .GetParameters()
        .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
        .ToArray();
    var writeLineMethod = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(String), typeof(Object)]);
    if (writeLineMethod is null)
    {
        Console.WriteLine($"Callback event detected but not attached: {eventInfo.Name}.");
        return;
    }

    var message = Expression.Call(
        writeLineMethod,
        Expression.Constant("Data callback: {0}"),
        Expression.Convert(parameters[0], typeof(Object)));
    var handler = Expression.Lambda(handlerType, message, parameters).Compile();
    eventInfo.AddEventHandler(client, handler);

    Console.WriteLine($"Callback event attached: {eventInfo.Name} ({handlerType.FullName}).");
}
