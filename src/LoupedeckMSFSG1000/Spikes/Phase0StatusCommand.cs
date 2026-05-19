namespace LoupedeckMSFSG1000.Spikes;

using Loupedeck;

public sealed class Phase0StatusCommand : PluginDynamicCommand
{
    private Int32 _pressCount;

    public Phase0StatusCommand()
        : base("Phase 0 Status", "Shows the current spike-only implementation status.", "Phase 0")
    {
    }

    protected override void RunCommand(String actionParameter)
    {
        _pressCount++;
        this.ActionImageChanged();
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        $"Phase 0{Environment.NewLine}{_pressCount}";
}
