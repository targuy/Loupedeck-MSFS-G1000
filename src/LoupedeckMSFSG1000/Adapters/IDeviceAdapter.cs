namespace LoupedeckMSFSG1000.Adapters;

using LoupedeckMSFSG1000.G1000;

public interface IDeviceAdapter
{
    String Name { get; }

    DeviceLedSupport LedSupport { get; }

    void OnPageChanged(G1000PageId pageId);
}
