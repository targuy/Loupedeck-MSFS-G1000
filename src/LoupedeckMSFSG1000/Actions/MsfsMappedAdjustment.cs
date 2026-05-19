namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.Msfs;
using LoupedeckMSFSG1000.Runtime;

public sealed class MsfsMappedAdjustment : PluginDynamicAdjustment
{
    public MsfsMappedAdjustment()
        : base("MSFS Encoder", "Generic MSFS encoder controls sent through WASim/K:Events.", "MSFS", hasReset: false)
    {
    }

    protected override Boolean OnLoad()
    {
        this.RemoveAllParameters();
        foreach (var adjustment in MsfsControlCatalog.Adjustments)
        {
            this.AddParameter(adjustment.Id, adjustment.Label, ToGroupName(adjustment.Group));
        }

        this.ParametersChanged();
        return true;
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (String.IsNullOrWhiteSpace(actionParameter))
        {
            actionParameter = "ap.hdg_bug";
        }

        var adjustment = MsfsControlCatalog.FindAdjustment(actionParameter);
        if (adjustment is null || diff == 0)
        {
            return;
        }

        _ = ExecuteAsync(adjustment, diff > 0 ? adjustment.IncrementCode : adjustment.DecrementCode, Math.Abs(diff));
    }

    protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override String GetAdjustmentValue(String actionParameter) => DisplayText.Hidden;

    protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
    {
        var adjustment = MsfsControlCatalog.FindAdjustment(actionParameter);
        return MsfsActionRenderer.RenderButton(adjustment?.Label ?? "MSFS", adjustment?.Group ?? MsfsControlGroup.Instruments, imageSize);
    }

    private static async Task ExecuteAsync(MsfsAdjustmentDefinition adjustment, String calculatorCode, Int32 repeatCount)
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            for (var i = 0; i < repeatCount; i++)
            {
                await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(calculatorCode);
            }

            PluginLog.Info($"MSFS adjustment sent: {adjustment.Id} x{repeatCount} -> {calculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"MSFS adjustment failed: {adjustment.Id}");
        }
    }

    private static String ToGroupName(MsfsControlGroup group) => $"MSFS - {group}";
}
