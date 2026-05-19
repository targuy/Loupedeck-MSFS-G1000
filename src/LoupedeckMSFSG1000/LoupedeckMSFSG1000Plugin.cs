namespace LoupedeckMSFSG1000;

using Loupedeck;
using LoupedeckMSFSG1000.Runtime;
using LoupedeckMSFSG1000.Sim;

public class LoupedeckMSFSG1000Plugin : Plugin
{
    public override Boolean UsesApplicationApiOnly => true;

    public override Boolean HasNoApplication => true;

    public LoupedeckMSFSG1000Plugin()
    {
        PluginLog.Init(this.Log);
        PluginResources.Init(this.Assembly);
    }

    public override void Load()
    {
        PluginLog.Info(WaSimReflectionClient.GetResolutionDiagnostics());
        if (WaSimReflectionClient.CanResolveClientLibrary())
        {
            PluginRuntime.Initialize(new WaSimReflectionClient());
            PluginLog.Info("WASimCommander client enabled.");
            _ = PluginRuntime.EnsureStateStartedAsync();
            return;
        }

        PluginRuntime.Initialize(new NullSimClient());
        PluginLog.Warning("WASimCommander client not found; simulator actions will be logged only.");
    }

    public override void Unload()
    {
        PluginRuntime.Shutdown();
    }
}
