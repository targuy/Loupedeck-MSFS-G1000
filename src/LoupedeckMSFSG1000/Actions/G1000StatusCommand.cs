namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.Runtime;
using LoupedeckMSFSG1000.Sim;

public sealed class G1000StatusCommand : PluginDynamicCommand
{
    public G1000StatusCommand()
        : base("G1000 Status", "Show MSFS/WASim connection status.", "Status")
    {
        PluginRuntime.ConnectionChanged += this.OnConnectionChanged;
    }

    protected override void RunCommand(String actionParameter) =>
        PluginRuntime.StartStateInBackground();

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        DisplayText.Hidden;

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        using var bitmap = new BitmapBuilder(imageSize);
        var state = PluginRuntime.ConnectionState;
        var accent = AccentFor(state);
        bitmap.Clear(BitmapColor.Black);
        bitmap.FillRectangle(0, 0, bitmap.Width, 5, accent);
        bitmap.DrawText("G1000", 4, 12, bitmap.Width - 8, 22, BitmapColor.White, 18, 24, 0, "Segoe UI Semibold");
        bitmap.DrawText(LabelFor(state), 4, 36, bitmap.Width - 8, 24, accent, 18, 24, 0, "Segoe UI Semibold");
        return bitmap.ToImage();
    }

    private void OnConnectionChanged(Object? sender, SimConnectionChangedEventArgs e) =>
        this.ActionImageChanged();

    private static String LabelFor(SimConnectionState state) => state switch
    {
        SimConnectionState.Unavailable => "NO WASIM",
        SimConnectionState.Disconnected => "WAIT",
        SimConnectionState.Connecting => "CONN",
        SimConnectionState.Connected => "ONLINE",
        SimConnectionState.Degraded => "DEGRADED",
        SimConnectionState.Reconnecting => "RETRY",
        SimConnectionState.Faulted => "FAULT",
        _ => "UNKNOWN",
    };

    private static BitmapColor AccentFor(SimConnectionState state) => state switch
    {
        SimConnectionState.Connected => new BitmapColor(0, 255, 101),
        SimConnectionState.Connecting or SimConnectionState.Reconnecting => new BitmapColor(0, 204, 255),
        SimConnectionState.Degraded or SimConnectionState.Faulted => new BitmapColor(255, 179, 0),
        SimConnectionState.Unavailable => new BitmapColor(255, 70, 70),
        _ => new BitmapColor(160, 160, 160),
    };
}
