namespace LoupedeckMSFSG1000.Spikes;

using Loupedeck;

public sealed class DynamicFolderSpike : PluginDynamicFolder
{
    private const String StatusAction = "Status";
    private const String LedProbeAction = "LedProbe";
    private const String ButtonFont = "Segoe UI Semibold";
    private const String CaptionFont = "Segoe UI";
    private Int32 _statusCounter;
    private Int32 _ledProbeCounter;

    public DynamicFolderSpike()
    {
        this.DisplayName = "G1000 Phase 0 Spike";
        this.Description = "Dynamic Folder spike for bitmap redraw and physical control event capture.";
        this.GroupName = "Phase 0";
        this.SupportedDevices = DeviceType.LoupedeckCtFamily;
    }

    public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType deviceType) =>
        PluginDynamicFolderNavigation.ButtonArea;

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
    {
        yield return this.CreateCommandName(StatusAction);
        yield return this.CreateCommandName(LedProbeAction);
    }

    public override IEnumerable<String> GetWheelToolNames(DeviceType deviceType)
    {
        yield return Phase0WheelTool.WheelTemplateName;
    }

    public override String GetButtonDisplayName(PluginImageSize imageSize) => "G1000 Spike";

    public override BitmapImage GetButtonImage(PluginImageSize imageSize)
    {
        using var bitmap = new BitmapBuilder(imageSize);
        bitmap.Clear(new BitmapColor(0, 85, 255));
        bitmap.FillRectangle(0, bitmap.Height - 10, bitmap.Width, 10, new BitmapColor(0, 40, 120));
        DrawText(bitmap, "G1000", 2, 8, bitmap.Width - 4, 26, 20);
        DrawText(bitmap, "SPIKE", 2, 38, bitmap.Width - 4, 14, 9, CaptionFont);
        return bitmap.ToImage();
    }

    public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            StatusAction => $"Status{Environment.NewLine}{_statusCounter}",
            LedProbeAction => $"LED/API{Environment.NewLine}{_ledProbeCounter}",
            _ => actionParameter,
        };

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var color = actionParameter == LedProbeAction
            ? new BitmapColor(255, 179, 0)
            : new BitmapColor(0, 204, 68);

        using var bitmap = new BitmapBuilder(imageSize);
        bitmap.Clear(color);
        bitmap.FillRectangle(0, 0, bitmap.Width, 8, new BitmapColor(20, 20, 20));

        if (actionParameter == LedProbeAction)
        {
            DrawText(bitmap, "LED/API", 2, 12, bitmap.Width - 4, 18, 13);
            DrawText(bitmap, _ledProbeCounter.ToString(), 2, 32, bitmap.Width - 4, 24, 20);
        }
        else
        {
            DrawText(bitmap, "STATUS", 2, 12, bitmap.Width - 4, 18, 13);
            DrawText(bitmap, _statusCounter.ToString(), 2, 32, bitmap.Width - 4, 24, 20);
        }

        return bitmap.ToImage();
    }

    public override void RunCommand(String actionParameter)
    {
        if (actionParameter == LedProbeAction)
        {
            _ledProbeCounter++;
            PluginLog.Warning("DynamicFolderSpike LED probe pressed. Physical RGB LED API remains unvalidated until device testing.");
        }
        else
        {
            _statusCounter++;
            PluginLog.Info("DynamicFolderSpike status pressed.");
        }

        this.CommandImageChanged(actionParameter);
    }

    private static void DrawText(BitmapBuilder bitmap, String text, Int32 x, Int32 y, Int32 width, Int32 height, Int32 fontSize) =>
        DrawText(bitmap, text, x, y, width, height, fontSize, ButtonFont);

    private static void DrawText(BitmapBuilder bitmap, String text, Int32 x, Int32 y, Int32 width, Int32 height, Int32 fontSize, String fontName) =>
        bitmap.DrawText(text, x, y, width, height, BitmapColor.White, fontSize, fontSize + 2, 0, fontName);
}
