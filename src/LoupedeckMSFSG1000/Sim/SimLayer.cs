namespace LoupedeckMSFSG1000.Sim;

public sealed class SimLayer : IDisposable
{
    private readonly ISimClient _client;

    public SimLayer(ISimClient client)
    {
        _client = client;
    }

    public event EventHandler<SimValueChangedEventArgs>
        ValueChanged
        {
            add => _client.ValueChanged += value;
            remove => _client.ValueChanged -= value;
        }

    public Boolean IsConnected => _client.IsConnected;

    public Task ConnectAsync(CancellationToken cancellationToken = default) =>
        _client.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        _client.DisconnectAsync(cancellationToken);

    public Task ExecuteCalculatorCodeAsync(String calculatorCode, CancellationToken cancellationToken = default) =>
        _client.ExecuteCalculatorCodeAsync(calculatorCode, cancellationToken);

    public Task SubscribeAsync(SimSubscription subscription, CancellationToken cancellationToken = default) =>
        _client.SubscribeAsync(subscription, cancellationToken);

    public void Dispose() => _client.Dispose();
}
