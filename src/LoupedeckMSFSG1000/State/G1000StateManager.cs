using LoupedeckMSFSG1000.Adapters;
using LoupedeckMSFSG1000.Sim;

namespace LoupedeckMSFSG1000.State;

public class G1000StateManager
{
    private readonly SimLayer _simLayer;
    private readonly IDeviceAdapter _adapter;

    public G1000StateManager(SimLayer simLayer, IDeviceAdapter adapter)
    {
        _simLayer = simLayer;
        _adapter = adapter;
    }

    public G1000State CurrentState { get; } = new();

    public event Action<StateChangeEvent>? StateChanged;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void OnAvionicsMasterChanged(bool isOn)
    {
        throw new NotImplementedException();
    }

    protected virtual void RaiseStateChanged(StateChangeEvent changeEvent) => StateChanged?.Invoke(changeEvent);
}
