namespace LoupedeckMSFSG1000.Sim;

public interface ISimClient : IDisposable
{
    event EventHandler<SimValueChangedEventArgs>? ValueChanged;

    Boolean IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken);

    Task DisconnectAsync(CancellationToken cancellationToken);

    Task ExecuteCalculatorCodeAsync(String calculatorCode, CancellationToken cancellationToken);

    Task SubscribeAsync(SimSubscription subscription, CancellationToken cancellationToken);
}
