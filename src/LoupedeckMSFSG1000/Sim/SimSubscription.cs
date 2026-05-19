namespace LoupedeckMSFSG1000.Sim;

public sealed record SimSubscription(
    String Id,
    String CalculatorCode,
    TimeSpan Interval,
    SimValueKind ValueKind);
