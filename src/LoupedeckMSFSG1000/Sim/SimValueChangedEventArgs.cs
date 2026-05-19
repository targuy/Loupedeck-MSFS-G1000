namespace LoupedeckMSFSG1000.Sim;

public sealed class SimValueChangedEventArgs : EventArgs
{
    public SimValueChangedEventArgs(String id, Object? value, DateTimeOffset timestamp)
    {
        this.Id = id;
        this.Value = value;
        this.Timestamp = timestamp;
    }

    public String Id { get; }

    public Object? Value { get; }

    public DateTimeOffset Timestamp { get; }
}
