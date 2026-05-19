namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;

internal static class G1000ActionRenderer
{
    private static readonly BitmapColor ActiveYellow = new(255, 198, 48);

    public static BitmapImage RenderButton(
        String label,
        G1000ControlPage page,
        PluginImageSize imageSize,
        String? valueText = null,
        Boolean? isActive = null,
        ActionDisplayStyle style = ActionDisplayStyle.Standard)
    {
        using var bitmap = new BitmapBuilder(imageSize);
        var isEncoder = style == ActionDisplayStyle.Encoder;
        var background = isActive == true && !isEncoder ? ActiveYellow : BitmapColor.Black;
        var foreground = isActive == true
            ? isEncoder ? ActiveYellow : BitmapColor.Black
            : BitmapColor.White;
        bitmap.Clear(background);

        switch (style)
        {
            case ActionDisplayStyle.Encoder:
                DrawTwoLine(bitmap, ShortName(label), valueText, foreground);
                break;
            case ActionDisplayStyle.BooleanButton:
                DrawButtonCenteredSingleLine(bitmap, CompactBooleanLabel(label), foreground);
                break;
            case ActionDisplayStyle.ValueButton:
                DrawButtonTwoLine(bitmap, CompactValueLabel(label), valueText, foreground);
                break;
            case ActionDisplayStyle.ApButton:
                DrawButtonThreeLine(bitmap, "AP", ApVariableName(label), valueText, foreground);
                break;
            case ActionDisplayStyle.Softkey:
                DrawSoftkey(bitmap, SoftkeyLabel(label), foreground);
                break;
            default:
                DrawStandard(bitmap, label, page, valueText, foreground);
                break;
        }

        return bitmap.ToImage();
    }

    private static void DrawTwoLine(BitmapBuilder bitmap, String title, String? valueText, BitmapColor color)
    {
        const Int32 titleHeight = 18;
        const Int32 valueHeight = 22;
        const Int32 gap = 3;
        var top = Math.Max(2, (bitmap.Height - titleHeight - valueHeight - gap) / 2);
        bitmap.DrawText(title, 5, top, bitmap.Width - 10, titleHeight, color, 14, 17, 0, "Segoe UI Semibold");
        bitmap.DrawText(String.IsNullOrWhiteSpace(valueText) ? "--" : valueText, 5, top + titleHeight + gap, bitmap.Width - 10, valueHeight, color, 16, 19, 0, "Segoe UI Semibold");
    }

    private static void DrawThreeLine(BitmapBuilder bitmap, String line1, String line2, String? line3, BitmapColor color)
    {
        const Int32 lineHeight = 17;
        const Int32 gap = 1;
        var top = Math.Max(2, (bitmap.Height - (lineHeight * 3) - (gap * 2)) / 2);
        bitmap.DrawText(line1, 6, top, bitmap.Width - 12, lineHeight, color, 15, 18, 0, "Segoe UI Semibold");
        bitmap.DrawText(line2, 6, top + lineHeight + gap, bitmap.Width - 12, lineHeight, color, 15, 18, 0, "Segoe UI Semibold");
        bitmap.DrawText(String.IsNullOrWhiteSpace(line3) ? "--" : line3, 6, top + ((lineHeight + gap) * 2), bitmap.Width - 12, lineHeight, color, 15, 18, 0, "Segoe UI Semibold");
    }

    private static void DrawButtonTwoLine(BitmapBuilder bitmap, String title, String? valueText, BitmapColor color)
    {
        const Int32 titleHeight = 26;
        const Int32 valueHeight = 31;
        const Int32 gap = 3;
        var top = Math.Max(2, (bitmap.Height - titleHeight - valueHeight - gap) / 2);
        bitmap.DrawText(title, 4, top, bitmap.Width - 8, titleHeight, color, 22, 27, 0, "Segoe UI Semibold");
        bitmap.DrawText(String.IsNullOrWhiteSpace(valueText) ? "--" : valueText, 4, top + titleHeight + gap, bitmap.Width - 8, valueHeight, color, 25, 31, 0, "Segoe UI Semibold");
    }

    private static void DrawButtonThreeLine(BitmapBuilder bitmap, String line1, String line2, String? line3, BitmapColor color)
    {
        const Int32 lineHeight = 23;
        const Int32 gap = 1;
        var top = Math.Max(2, (bitmap.Height - (lineHeight * 3) - (gap * 2)) / 2);
        bitmap.DrawText(line1, 4, top, bitmap.Width - 8, lineHeight, color, 22, 28, 0, "Segoe UI Semibold");
        bitmap.DrawText(line2, 4, top + lineHeight + gap, bitmap.Width - 8, lineHeight, color, 22, 28, 0, "Segoe UI Semibold");
        bitmap.DrawText(String.IsNullOrWhiteSpace(line3) ? "--" : line3, 4, top + ((lineHeight + gap) * 2), bitmap.Width - 8, lineHeight, color, 22, 28, 0, "Segoe UI Semibold");
    }

    private static void DrawButtonCenteredSingleLine(BitmapBuilder bitmap, String text, BitmapColor color)
    {
        const Int32 textHeight = 42;
        var top = Math.Max(2, (bitmap.Height - textHeight) / 2);
        bitmap.DrawText(text, 4, top, bitmap.Width - 8, textHeight, color, 27, 32, 0, "Segoe UI Semibold");
    }

    private static void DrawSoftkey(BitmapBuilder bitmap, String label, BitmapColor color)
    {
        bitmap.DrawText(label, 2, 10, bitmap.Width - 4, 18, color, 12, 15, 0, "Segoe UI Semibold");
        bitmap.DrawText("▲", 2, 31, bitmap.Width - 4, 20, color, 18, 22, 0, "Segoe UI Semibold");
    }

    private static void DrawStandard(BitmapBuilder bitmap, String label, G1000ControlPage page, String? valueText, BitmapColor color)
    {
        if (!String.IsNullOrWhiteSpace(valueText))
        {
            DrawButtonTwoLine(bitmap, ShortName(label), valueText, color);
            return;
        }

        var accent = G1000ControlCatalog.PageColor(page);
        bitmap.FillRectangle(0, 0, bitmap.Width, 5, accent);
        bitmap.DrawText(label, 4, 17, bitmap.Width - 8, 38, color, 21, 27, 0, "Segoe UI Semibold");
    }

    private static String ShortName(String label) => label switch
    {
        "ALT Select" => "ALT",
        "VS Select" => "VS",
        "HDG Bug" => "HDG",
        "Elev Trim" => "TRIM",
        "G1000 BARO" => "BARO",
        _ => label.Replace(" Select", String.Empty, StringComparison.Ordinal).Replace(" Bug", String.Empty, StringComparison.Ordinal),
    };

    private static String ApVariableName(String label) => label.Replace("AP ", String.Empty, StringComparison.Ordinal);

    private static String CompactBooleanLabel(String label) => label switch
    {
        "Gear Toggle" => "GEAR",
        "Parking Brake" => "PARK",
        "Landing Lights" => "LAND",
        "Nav Lights" => "NAV",
        "Taxi Lights" => "TAXI",
        _ => label.ToUpperInvariant(),
    };

    private static String CompactValueLabel(String label)
    {
        if (label.StartsWith("Flaps", StringComparison.Ordinal))
        {
            return "FLAPS";
        }

        return ShortName(label).ToUpperInvariant();
    }

    private static String SoftkeyLabel(String label) =>
        label.StartsWith("PFD Softkey ", StringComparison.Ordinal)
            ? $"SK {label["PFD Softkey ".Length..]}"
            : label;
}
