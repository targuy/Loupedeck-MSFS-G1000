namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.Msfs;
using LoupedeckMSFSG1000.Runtime;

public sealed class MsfsMappedCommand : PluginDynamicCommand
{
    public MsfsMappedCommand()
        : base("MSFS Command", "Generic MSFS commands sent through WASim/K:Events.", "MSFS")
    {
    }

    protected override Boolean OnLoad()
    {
        this.RemoveAllParameters();
        foreach (var command in MsfsControlCatalog.Commands)
        {
            this.AddParameter(command.Id, command.Label, ToGroupName(command.Group));
        }

        this.ParametersChanged();
        return true;
    }

    protected override void RunCommand(String actionParameter)
    {
        if (String.IsNullOrWhiteSpace(actionParameter))
        {
            actionParameter = "ap.master";
        }

        var command = MsfsControlCatalog.FindCommand(actionParameter);
        if (command is null)
        {
            PluginLog.Warning($"Unknown MSFS command parameter '{actionParameter}'.");
            return;
        }

        _ = ExecuteAsync(command);
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var command = MsfsControlCatalog.FindCommand(actionParameter);
        return MsfsActionRenderer.RenderButton(command?.Label ?? "MSFS", command?.Group ?? MsfsControlGroup.Instruments, imageSize);
    }

    private static async Task ExecuteAsync(MsfsCommandDefinition command)
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(command.CalculatorCode);
            PluginLog.Info($"MSFS command sent: {command.Id} -> {command.CalculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"MSFS command failed: {command.Id}");
        }
    }

    private static String ToGroupName(MsfsControlGroup group) => $"MSFS - {group}";
}
