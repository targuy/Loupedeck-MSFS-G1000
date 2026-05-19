namespace LoupedeckMSFSG1000.State;

using LoupedeckMSFSG1000.Sim;

public sealed class G1000StateManager : IDisposable
{
    private readonly SimLayer _simLayer;
    private readonly HashSet<String> _subscribedIds = [];

    public G1000StateManager(SimLayer simLayer)
    {
        _simLayer = simLayer;
        _simLayer.ValueChanged += this.OnSimValueChanged;
    }

    public event EventHandler<G1000StateChangedEventArgs>? StateChanged;

    public G1000State Current { get; private set; } = new();

    public void SetLocalValue(String id, Double value)
    {
        var values = new Dictionary<String, Double>(this.Current.Values)
        {
            [id] = value,
        };

        this.SetState(this.BuildState(values));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var successCount = 0;
        var failureCount = 0;
        foreach (var subscription in SimVariables.Subscriptions)
        {
            if (_subscribedIds.Contains(subscription.Id))
            {
                continue;
            }

            try
            {
                await _simLayer.SubscribeAsync(subscription, cancellationToken);
                _subscribedIds.Add(subscription.Id);
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                PluginLog.Warning($"WASim subscription skipped: {subscription.Id} -> {subscription.CalculatorCode}; {ex.Message}");
            }
        }

        if (successCount == 0 && failureCount > 0)
        {
            throw new InvalidOperationException($"No WASim subscriptions could be added ({failureCount} failed).");
        }

        if (failureCount > 0)
        {
            PluginLog.Warning($"WASim subscriptions partially started: {successCount} added, {failureCount} failed.");
            throw new InvalidOperationException($"WASim subscriptions incomplete: {successCount} added, {failureCount} failed.");
        }
    }

    private void OnSimValueChanged(Object? sender, SimValueChangedEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }

        var value = Convert.ToDouble(e.Value);
        var values = new Dictionary<String, Double>(this.Current.Values)
        {
            [e.Id] = value,
        };

        this.SetState(this.BuildState(values));
    }

    private G1000State BuildState(IReadOnlyDictionary<String, Double> values) =>
        this.Current with
        {
            Values = values,
            AutopilotMaster = GetBoolean(values, "ap.master"),
            HeadingMode = GetBoolean(values, "ap.hdg"),
            NavMode = GetBoolean(values, "ap.nav"),
            AltitudeMode = GetBoolean(values, "ap.alt"),
            Com1ActiveMhz = GetDouble(values, "com1.active"),
            Com1StandbyMhz = GetDouble(values, "com1.stby"),
        };

    private static Boolean GetBoolean(IReadOnlyDictionary<String, Double> values, String id) =>
        values.TryGetValue(id, out var value) && value > 0.5;

    private static Double GetDouble(IReadOnlyDictionary<String, Double> values, String id) =>
        values.TryGetValue(id, out var value) ? value : 0.0;

    private void SetState(G1000State next)
    {
        var previous = this.Current;
        if (previous == next)
        {
            return;
        }

        this.Current = next;
        this.StateChanged?.Invoke(this, new G1000StateChangedEventArgs(previous, next));
    }

    public void Dispose() => _simLayer.ValueChanged -= this.OnSimValueChanged;
}
