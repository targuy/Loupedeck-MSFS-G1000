namespace LoupedeckMSFSG1000.G1000;

public sealed record G1000CommandDefinition(
    String Id,
    String Label,
    String CalculatorCode,
    G1000ControlPage Page,
    String? StateId = null);
