namespace LoupedeckMSFSG1000.Spikes;

using Loupedeck;
using Loupedeck.Devices.Loupedeck2Devices;

public sealed class Phase0WheelTool : WheelTool
{
    public const String WheelTemplateName = "LoupedeckMSFSG1000.Phase0Wheel";

    private static readonly (String Label, BitmapColor Color)[] Pages =
    [
        ("PFD", new BitmapColor(0, 85, 255)),
        ("MFD", new BitmapColor(0, 204, 68)),
        ("AP", new BitmapColor(255, 179, 0)),
        ("COM", new BitmapColor(0, 204, 255)),
    ];

    private Int32 _pageIndex;

    public Phase0WheelTool()
        : base(WheelTemplateName, "G1000 Wheel Spike")
    {
        this.TemplateDescription = "Phase 0 CT wheel screen rendering probe.";
        this.LockOnStart = false;
    }

    protected override BitmapImage CreateImage() => this.RenderImage();

    protected override BitmapImage CreateDemoImage() => this.RenderImage();

    protected override void OnEncoderEvent(DeviceEncoderEvent encoderEvent)
    {
        if (encoderEvent.Clicks != 0)
        {
            _pageIndex = PositiveModulo(_pageIndex + Math.Sign(encoderEvent.Clicks), Pages.Length);
            this.Draw();
            PluginLog.Info($"Phase0WheelTool page changed to {Pages[_pageIndex].Label}.");
        }
    }

    private BitmapImage RenderImage()
    {
        var page = Pages[_pageIndex];
        using var bitmap = new BitmapBuilder(240, 240);
        bitmap.Clear(page.Color);
        bitmap.FillRectangle(0, 0, bitmap.Width, 28, new BitmapColor(20, 20, 20));
        bitmap.DrawText("G1000", 0, 4, bitmap.Width, 20, BitmapColor.White, 14, 16, 0, "Segoe UI Semibold");
        bitmap.DrawText(page.Label, 0, 78, bitmap.Width, 64, BitmapColor.White, 42, 46, 0, "Segoe UI Semibold");
        bitmap.DrawText("WHEEL", 0, 148, bitmap.Width, 24, BitmapColor.White, 14, 16, 0, "Segoe UI");
        bitmap.FillCircle(96, 206, 5, _pageIndex == 0 ? BitmapColor.White : new BitmapColor(70, 70, 70));
        bitmap.FillCircle(112, 206, 5, _pageIndex == 1 ? BitmapColor.White : new BitmapColor(70, 70, 70));
        bitmap.FillCircle(128, 206, 5, _pageIndex == 2 ? BitmapColor.White : new BitmapColor(70, 70, 70));
        bitmap.FillCircle(144, 206, 5, _pageIndex == 3 ? BitmapColor.White : new BitmapColor(70, 70, 70));
        return bitmap.ToImage();
    }

    private static Int32 PositiveModulo(Int32 value, Int32 modulo) => (value % modulo + modulo) % modulo;
}
