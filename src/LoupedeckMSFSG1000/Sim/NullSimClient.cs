namespace LoupedeckMSFSG1000.Sim;

public sealed class NullSimClient : ISimClient
{
    public event EventHandler<SimValueChangedEventArgs>? ValueChanged;

    public Boolean IsConnected => false;

    public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ExecuteCalculatorCodeAsync(String calculatorCode, CancellationToken cancellationToken)
    {
        this.ValueChanged?.Invoke(this, new SimValueChangedEventArgs(calculatorCode, null, DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(SimSubscription subscription, CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose()
    {
    }
}
