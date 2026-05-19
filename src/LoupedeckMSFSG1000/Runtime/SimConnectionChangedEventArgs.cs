namespace LoupedeckMSFSG1000.Runtime;

using LoupedeckMSFSG1000.Sim;

public sealed class SimConnectionChangedEventArgs : EventArgs
{
    public SimConnectionChangedEventArgs(SimConnectionState previous, SimConnectionState current, String? message)
    {
        this.Previous = previous;
        this.Current = current;
        this.Message = message;
    }

    public SimConnectionState Previous { get; }

    public SimConnectionState Current { get; }

    public String? Message { get; }
}
