namespace LoupedeckMSFSG1000.State;

public record StateChangeEvent(string PropertyName, object? OldValue, object? NewValue);
