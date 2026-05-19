namespace LoupedeckMSFSG1000.State;

public sealed record G1000State
{
    public IReadOnlyDictionary<String, Double> Values { get; init; } = new Dictionary<String, Double>();

    public Boolean AvionicsOn { get; init; }

    public Boolean AutopilotMaster { get; init; }

    public Boolean HeadingMode { get; init; }

    public Boolean NavMode { get; init; }

    public Boolean AltitudeMode { get; init; }

    public Double Com1ActiveMhz { get; init; }

    public Double Com1StandbyMhz { get; init; }

    public Double? GetValue(String? id) =>
        id is not null && this.Values.TryGetValue(id, out var value)
            ? value
            : null;
}
