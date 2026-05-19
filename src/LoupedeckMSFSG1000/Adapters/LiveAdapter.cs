namespace LoupedeckMSFSG1000.Adapters;

using LoupedeckMSFSG1000.G1000;

public sealed class LiveAdapter : IDeviceAdapter
{
    public String Name => "Loupedeck Live";

    public DeviceLedSupport LedSupport => DeviceLedSupport.PhysicalActionDependent;

    public void OnPageChanged(G1000PageId pageId)
    {
        PluginLog.Info($"LiveAdapter page changed to {pageId}; physical LED behavior remains action-dependent.");
    }
}
