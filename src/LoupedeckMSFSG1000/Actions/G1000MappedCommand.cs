namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Runtime;

public sealed class G1000MappedCommand : PluginDynamicCommand
{
    public G1000MappedCommand()
        : base("G1000 Button", "G1000 mapped button commands.", "G1000")
    {
    }

    protected override Boolean OnLoad()
    {
        this.RemoveAllParameters();
        foreach (var command in G1000ControlCatalog.Commands)
        {
            this.AddParameter(command.Id, command.Label, PageGroup(command.Page));
        }

        this.ParametersChanged();
        return true;
    }

    protected override void RunCommand(String actionParameter)
    {
        if (String.IsNullOrWhiteSpace(actionParameter))
        {
            actionParameter = "fixed.directto";
        }

        var command = G1000ControlCatalog.FindCommand(actionParameter);
        if (command is null)
        {
            PluginLog.Warning($"Unknown G1000 command parameter '{actionParameter}'.");
            return;
        }

        _ = ExecuteAsync(command);
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var command = G1000ControlCatalog.FindCommand(actionParameter);
        return G1000ActionRenderer.RenderButton(command?.Label ?? "G1000", command?.Page ?? G1000ControlPage.Fixed, imageSize);
    }

    private static async Task ExecuteAsync(G1000CommandDefinition command)
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(command.CalculatorCode);
            PluginLog.Info($"G1000 command sent: {command.Id} -> {command.CalculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"G1000 command failed: {command.Id}");
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
