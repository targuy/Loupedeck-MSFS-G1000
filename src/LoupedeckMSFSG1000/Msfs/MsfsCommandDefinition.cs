namespace LoupedeckMSFSG1000.Msfs;

public sealed record MsfsCommandDefinition(
    String Id,
    String Label,
    String CalculatorCode,
    MsfsControlGroup Group,
    String? StateId = null);
