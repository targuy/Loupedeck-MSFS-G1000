namespace LoupedeckMSFSG1000.Runtime;

using LoupedeckMSFSG1000.Sim;
using LoupedeckMSFSG1000.State;

internal static class PluginRuntime
{
    private static readonly Object SyncRoot = new();
    private static SimLayer? _simLayer;
    private static G1000StateManager? _stateManager;
    private static Boolean _stateStarted;
    private static Task? _stateStartTask;
    private static CancellationTokenSource? _stateCancellation;
    private static DateTimeOffset _lastStateStartFailureUtc = DateTimeOffset.MinValue;
    private static SimConnectionState _connectionState = SimConnectionState.Unavailable;
    private static String? _connectionMessage = "Plugin not initialized";

    public static event EventHandler<G1000StateChangedEventArgs>? StateChanged;

    public static event EventHandler<SimConnectionChangedEventArgs>? ConnectionChanged;

    public static SimLayer SimLayer
    {
        get
        {
            lock (SyncRoot)
            {
                return _simLayer ??= new SimLayer(new NullSimClient());
            }
        }
    }

    public static G1000State State
    {
        get
        {
            lock (SyncRoot)
            {
                return _stateManager?.Current ?? new G1000State();
            }
        }
    }

    public static SimConnectionState ConnectionState
    {
        get
        {
            lock (SyncRoot)
            {
                return _connectionState;
            }
        }
    }

    public static String? ConnectionMessage
    {
        get
        {
            lock (SyncRoot)
            {
                return _connectionMessage;
            }
        }
    }

    public static void Initialize(ISimClient simClient)
    {
        lock (SyncRoot)
        {
            _stateCancellation?.Cancel();
            _stateCancellation?.Dispose();
            _stateCancellation = new CancellationTokenSource();

            if (_stateManager is not null)
            {
                _stateManager.StateChanged -= OnStateChanged;
                _stateManager.Dispose();
                _stateManager = null;
            }

            _simLayer?.Dispose();
            _simLayer = new SimLayer(simClient);
            _stateManager = new G1000StateManager(_simLayer);
            _stateManager.StateChanged += OnStateChanged;
            _stateStarted = false;
            _stateStartTask = null;
            _lastStateStartFailureUtc = DateTimeOffset.MinValue;
            SetConnectionStateLocked(
                simClient is NullSimClient ? SimConnectionState.Unavailable : SimConnectionState.Disconnected,
                simClient is NullSimClient ? "WASim client unavailable" : "Waiting for MSFS/WASim");
        }
    }

    public static async Task EnsureStateStartedAsync(CancellationToken cancellationToken = default)
    {
        Task? startTask;
        lock (SyncRoot)
        {
            if (_stateStarted)
            {
                return;
            }

            if (_stateStartTask is { IsCompleted: false })
            {
                startTask = _stateStartTask;
            }
            else
            {
                if (_connectionState == SimConnectionState.Unavailable)
                {
                    return;
                }

                if (DateTimeOffset.UtcNow - _lastStateStartFailureUtc < TimeSpan.FromSeconds(15))
                {
                    return;
                }

                var stateManager = _stateManager;
                if (stateManager is null)
                {
                    return;
                }

                SetConnectionStateLocked(
                    _connectionState == SimConnectionState.Faulted ? SimConnectionState.Reconnecting : SimConnectionState.Connecting,
                    "Connecting to MSFS/WASim");

                var linkedToken = cancellationToken == default
                    ? _stateCancellation?.Token ?? CancellationToken.None
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stateCancellation?.Token ?? CancellationToken.None).Token;
                startTask = _stateStartTask = StartStateManagerAsync(stateManager, linkedToken);
            }
        }

        try
        {
            await startTask.ConfigureAwait(false);
        }
        catch
        {
            // StartStateManagerAsync logs and swallows operational errors.
        }
    }

    public static void StartStateInBackground()
    {
        _ = EnsureStateStartedAsync();
    }

    public static void SetLocalStateValue(String id, Double value)
    {
        lock (SyncRoot)
        {
            _stateManager?.SetLocalValue(id, value);
        }
    }

    private static async Task StartStateManagerAsync(G1000StateManager stateManager, CancellationToken cancellationToken)
    {
        try
        {
            await stateManager.StartAsync(cancellationToken).ConfigureAwait(false);
            lock (SyncRoot)
            {
                if (ReferenceEquals(_stateManager, stateManager))
                {
                    _stateStarted = true;
                    _stateStartTask = null;
                    _lastStateStartFailureUtc = DateTimeOffset.MinValue;
                    SetConnectionStateLocked(SimConnectionState.Connected, "MSFS/WASim connected");
                }
            }
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                PluginLog.Info("G1000 state subscriptions cancelled.");
            }
            else
            {
                PluginLog.Error(ex, "G1000 state subscriptions failed.");
            }

            lock (SyncRoot)
            {
                if (ReferenceEquals(_stateManager, stateManager))
                {
                    _stateStartTask = null;
                    _lastStateStartFailureUtc = cancellationToken.IsCancellationRequested
                        ? DateTimeOffset.MinValue
                        : DateTimeOffset.UtcNow;
                    SetConnectionStateLocked(
                        cancellationToken.IsCancellationRequested ? SimConnectionState.Disconnected : SimConnectionState.Faulted,
                        cancellationToken.IsCancellationRequested ? "Connection cancelled" : ex.Message);
                }
            }
        }
    }

    public static void Shutdown()
    {
        lock (SyncRoot)
        {
            _stateCancellation?.Cancel();
            _stateCancellation?.Dispose();
            _stateCancellation = null;

            if (_stateManager is not null)
            {
                _stateManager.StateChanged -= OnStateChanged;
                _stateManager.Dispose();
                _stateManager = null;
            }

            _simLayer?.Dispose();
            _simLayer = null;
            _stateStarted = false;
            _stateStartTask = null;
            SetConnectionStateLocked(SimConnectionState.Disconnected, "Plugin shutdown");
        }
    }

    private static void OnStateChanged(Object? sender, G1000StateChangedEventArgs e) =>
        StateChanged?.Invoke(sender, e);

    private static void SetConnectionStateLocked(SimConnectionState state, String? message)
    {
        var previous = _connectionState;
        _connectionState = state;
        _connectionMessage = message;
        if (previous != state)
        {
            ConnectionChanged?.Invoke(
                null,
                new SimConnectionChangedEventArgs(previous, state, message));
        }
    }
}
