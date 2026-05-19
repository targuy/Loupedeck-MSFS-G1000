using LoupedeckMSFSG1000.Sim;

namespace LoupedeckMSFSG1000.G1000.Controls;

public class DualRingEncoder
{
    private readonly SimLayer _simLayer;

    public DualRingEncoder(SimLayer simLayer)
    {
        _simLayer = simLayer;
    }

    public RingMode Mode { get; private set; } = RingMode.Outer;

    public enum RingMode
    {
        Outer,
        Inner
    }

    public void OnClick()
    {
        Mode = Mode == RingMode.Outer ? RingMode.Inner : RingMode.Outer;
        throw new NotImplementedException();
    }

    public void OnRotate(int delta)
    {
        throw new NotImplementedException();
    }
}
