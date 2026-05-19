namespace LoupedeckMSFSG1000.G1000;

public sealed record G1000AdjustmentDefinition(
    String Id,
    String Label,
    String IncrementCode,
    String DecrementCode,
    G1000ControlPage Page,
    String? ValueId = null,
    String? ResetCode = null);
