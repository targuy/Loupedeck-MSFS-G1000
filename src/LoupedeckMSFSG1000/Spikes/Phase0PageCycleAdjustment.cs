namespace LoupedeckMSFSG1000.Spikes;

using Loupedeck;

public sealed class Phase0PageCycleAdjustment : PluginDynamicAdjustment
{
    private static readonly (String Label, BitmapColor Color)[] Pages =
    [
        ("PFD", new BitmapColor(0, 85, 255)),
        ("MFD", new BitmapColor(0, 204, 68)),
        ("AP", new BitmapColor(255, 179, 0)),
        ("COM", new BitmapColor(0, 204, 255)),
    ];

    private Int32 _pageIndex;

    public Phase0PageCycleAdjustment()
        : base("G1000 Page Cycle", "Cycles G1000 pages from a CT wheel page.", "Phase 0", hasReset: true)
    {
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (diff != 0)
        {
            _pageIndex = PositiveModulo(_pageIndex + Math.Sign(diff), Pages.Length);
            PluginLog.Info($"Phase0PageCycleAdjustment changed to {Pages[_pageIndex].Label}.");
            this.AdjustmentValueChanged();
        }
    }

    protected override void RunCommand(String actionParameter)
    {
        _pageIndex = 0;
        PluginLog.Info("Phase0PageCycleAdjustment reset to PFD.");
        this.AdjustmentValueChanged();
    }

    protected override String GetAdjustmentValue(String actionParameter) => Pages[_pageIndex].Label;

    protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) =>
        $"G1000{Environment.NewLine}{Pages[_pageIndex].Label}";

    protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        Render(Pages[_pageIndex].Label, Pages[_pageIndex].Color, imageSize);

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
        Render("RESET", new BitmapColor(26, 26, 26), imageSize);

    private static BitmapImage Render(String label, BitmapColor color, PluginImageSize imageSize)
    {
        using var bitmap = new BitmapBuilder(imageSize);
        bitmap.Clear(color);
        bitmap.FillRectangle(0, 0, bitmap.Width, 8, new BitmapColor(20, 20, 20));
        bitmap.DrawText("G1000", 2, 12, bitmap.Width - 4, 18, BitmapColor.White, 13, 15, 0, "Segoe UI Semibold");
        bitmap.DrawText(label, 2, 32, bitmap.Width - 4, 24, BitmapColor.White, 20, 22, 0, "Segoe UI Semibold");
        return bitmap.ToImage();
    }

    private static Int32 PositiveModulo(Int32 value, Int32 modulo) => (value % modulo + modulo) % modulo;
}
