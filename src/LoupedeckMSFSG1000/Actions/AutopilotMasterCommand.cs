namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Runtime;
using LoupedeckMSFSG1000.Sim;
using LoupedeckMSFSG1000.State;

public sealed class AutopilotMasterCommand : PluginDynamicCommand
{
    public AutopilotMasterCommand()
        : base("AP Master", "Toggles MSFS autopilot master through SimLayer.", "G1000")
    {
        PluginRuntime.StateChanged += this.OnStateChanged;
    }

    protected override void RunCommand(String actionParameter)
    {
        _ = this.ToggleAutopilotMasterAsync();
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var isOn = PluginRuntime.State.AutopilotMaster;
        return G1000ActionRenderer.RenderButton("AP Master", G1000ControlPage.Autopilot, imageSize, isOn ? "ON" : "OFF", isOn, ActionDisplayStyle.ApButton);
    }

    private async Task ToggleAutopilotMasterAsync()
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(SimVariables.AutopilotMasterToggle);
            PluginRuntime.SetLocalStateValue("ap.master", PluginRuntime.State.AutopilotMaster ? 0.0 : 1.0);
            this.ActionImageChanged();
            PluginLog.Info("AP Master command sent through SimLayer.");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "AP Master command failed.");
        }
    }

    private void OnStateChanged(Object? sender, G1000StateChangedEventArgs e)
    {
        if (e.Previous.AutopilotMaster != e.Current.AutopilotMaster)
        {
            this.ActionImageChanged();
        }
    }
}
