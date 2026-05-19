namespace LoupedeckMSFSG1000.Sim;

public enum SimConnectionState
{
    Unavailable,
    Disconnected,
    Connecting,
    Connected,
    Degraded,
    Reconnecting,
    Faulted,
}
