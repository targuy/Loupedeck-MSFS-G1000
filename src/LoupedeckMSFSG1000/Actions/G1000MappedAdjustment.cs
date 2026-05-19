namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Runtime;

public sealed class G1000MappedAdjustment : PluginDynamicAdjustment
{
    public G1000MappedAdjustment()
        : base("G1000 Encoder", "G1000 mapped encoder controls.", "G1000", hasReset: true)
    {
    }

    protected override Boolean OnLoad()
    {
        this.RemoveAllParameters();
        foreach (var adjustment in G1000ControlCatalog.Adjustments)
        {
            this.AddParameter(adjustment.Id, adjustment.Label, PageGroup(adjustment.Page));
        }

        this.ParametersChanged();
        return true;
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (String.IsNullOrWhiteSpace(actionParameter))
        {
            actionParameter = "pfd.baro";
        }

        var adjustment = G1000ControlCatalog.FindAdjustment(actionParameter);
        if (adjustment is null || diff == 0)
        {
            return;
        }

        _ = ExecuteAsync(adjustment, diff > 0 ? adjustment.IncrementCode : adjustment.DecrementCode, Math.Abs(diff));
    }

    protected override void RunCommand(String actionParameter)
    {
        var adjustment = G1000ControlCatalog.FindAdjustment(actionParameter);
        if (adjustment?.ResetCode is null)
        {
            return;
        }

        _ = ExecuteAsync(adjustment, adjustment.ResetCode, 1);
    }

    protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override String GetAdjustmentValue(String actionParameter) => DisplayText.Hidden;

    protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
    {
        var adjustment = G1000ControlCatalog.FindAdjustment(actionParameter);
        return G1000ActionRenderer.RenderButton(adjustment?.Label ?? "G1000", adjustment?.Page ?? G1000ControlPage.Fixed, imageSize);
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var adjustment = G1000ControlCatalog.FindAdjustment(actionParameter);
        return G1000ActionRenderer.RenderButton("PUSH", adjustment?.Page ?? G1000ControlPage.Fixed, imageSize);
    }

    private static async Task ExecuteAsync(G1000AdjustmentDefinition adjustment, String calculatorCode, Int32 repeatCount)
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            for (var i = 0; i < repeatCount; i++)
            {
                await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(calculatorCode);
            }

            PluginLog.Info($"G1000 adjustment sent: {adjustment.Id} x{repeatCount} -> {calculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"G1000 adjustment failed: {adjustment.Id}");
        }
    }

    private static String PageGroup(G1000ControlPage page) => page switch
    {
        G1000ControlPage.Pfd => "G1000 - PFD",
        G1000ControlPage.Mfd => "G1000 - MFD",
        G1000ControlPage.Autopilot => "G1000 - Autopilot",
        G1000ControlPage.ComNav => "G1000 - COM/NAV",
        _ => "G1000 - Fixed",
    };
}
