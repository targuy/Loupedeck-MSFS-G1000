namespace LoupedeckMSFSG1000.Sim;

public class SimConnectClient
{
    public event Action? AircraftLoaded;
    public event Action? SimStarted;
    public event Action? SimStopped;
    public event Action<bool>? SimPaused;

    public void Initialize()
    {
        throw new NotImplementedException();
    }

    public void Shutdown()
    {
        throw new NotImplementedException();
    }

    public void RaiseAircraftLoaded() => AircraftLoaded?.Invoke();
    public void RaiseSimStarted() => SimStarted?.Invoke();
    public void RaiseSimStopped() => SimStopped?.Invoke();
    public void RaiseSimPaused(bool paused) => SimPaused?.Invoke(paused);
}
