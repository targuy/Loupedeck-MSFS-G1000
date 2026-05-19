namespace LoupedeckMSFSG1000.State;

public sealed class G1000StateChangedEventArgs : EventArgs
{
    public G1000StateChangedEventArgs(G1000State previous, G1000State current)
    {
        this.Previous = previous;
        this.Current = current;
    }

    public G1000State Previous { get; }

    public G1000State Current { get; }
}
