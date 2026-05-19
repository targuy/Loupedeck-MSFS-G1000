namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.Msfs;

internal static class MsfsActionRenderer
{
    public static BitmapImage RenderButton(String label, MsfsControlGroup group, PluginImageSize imageSize)
    {
        var color = MsfsControlCatalog.GroupColor(group);
        using var bitmap = new BitmapBuilder(imageSize);
        bitmap.Clear(new BitmapColor(20, 24, 29));
        bitmap.FillRectangle(0, 0, bitmap.Width, 7, color);

        var parts = label.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length <= 1)
        {
            bitmap.DrawText(label, 2, 18, bitmap.Width - 4, 24, BitmapColor.White, 16, 20, 0, "Segoe UI Semibold");
        }
        else
        {
            bitmap.DrawText(parts[0], 2, 11, bitmap.Width - 4, 20, BitmapColor.White, 14, 17, 0, "Segoe UI Semibold");
            bitmap.DrawText(String.Join(' ', parts.Skip(1)), 2, 32, bitmap.Width - 4, 18, new BitmapColor(210, 216, 224), 10, 12, 0, "Segoe UI");
        }

        return bitmap.ToImage();
    }
}
