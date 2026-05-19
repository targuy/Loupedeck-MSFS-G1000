namespace LoupedeckMSFSG1000.Msfs;

public sealed record MsfsAdjustmentDefinition(
    String Id,
    String Label,
    String IncrementCode,
    String DecrementCode,
    MsfsControlGroup Group,
    String? ValueId = null,
    String? ResetCode = null);
