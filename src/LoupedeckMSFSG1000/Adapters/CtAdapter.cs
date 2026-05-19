namespace LoupedeckMSFSG1000.Adapters;

using LoupedeckMSFSG1000.G1000;

public sealed class CtAdapter : IDeviceAdapter
{
    public String Name => "Loupedeck CT";

    public DeviceLedSupport LedSupport => DeviceLedSupport.PhysicalActionDependent;

    public void OnPageChanged(G1000PageId pageId)
    {
        PluginLog.Info($"CtAdapter page changed to {pageId}; CT wheel integration uses dynamic adjustments.");
    }
}
