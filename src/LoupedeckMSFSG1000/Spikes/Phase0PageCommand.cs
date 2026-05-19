namespace LoupedeckMSFSG1000.Spikes;

using Loupedeck;

public sealed class Phase0PageCommand : PluginDynamicCommand
{
    private static readonly Dictionary<String, (String Label, BitmapColor Color)> Pages = new()
    {
        ["PFD"] = ("PFD", new BitmapColor(0, 85, 255)),
        ["MFD"] = ("MFD", new BitmapColor(0, 204, 68)),
        ["AP"] = ("AP", new BitmapColor(255, 179, 0)),
        ["COM"] = ("COM", new BitmapColor(0, 204, 255)),
    };

    private String _selectedPage = "PFD";

    public Phase0PageCommand()
        : base("G1000 Page Button", "Page buttons for CT wheel page templates.", "Phase 0")
    {
    }

    protected override void RunCommand(String actionParameter)
    {
        if (Pages.ContainsKey(actionParameter))
        {
            _selectedPage = actionParameter;
            PluginLog.Info($"Phase0PageCommand selected {_selectedPage}.");
            this.ActionImageChanged();
        }
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        Pages.TryGetValue(actionParameter, out var page) ? page.Label : "G1000";

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var page = Pages.TryGetValue(actionParameter, out var value)
            ? value
            : Pages[_selectedPage];

        using var bitmap = new BitmapBuilder(imageSize);
        bitmap.Clear(page.Color);
        bitmap.DrawText(page.Label, 2, 18, bitmap.Width - 4, 30, BitmapColor.White, 20, 22, 0, "Segoe UI Semibold");
        return bitmap.ToImage();
    }
}
