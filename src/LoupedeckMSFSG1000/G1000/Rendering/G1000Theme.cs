using LoupedeckMSFSG1000.Common;

namespace LoupedeckMSFSG1000.G1000.Rendering;

public static class G1000Theme
{
    public static readonly PageTheme Pfd = new("PFD", new BitmapColor(0, 85, 255), new BitmapColor(0, 25, 76), PhysicalButton.B5);
    public static readonly PageTheme Mfd = new("MFD", new BitmapColor(0, 204, 68), new BitmapColor(0, 61, 20), PhysicalButton.B6);
    public static readonly PageTheme Autopilot = new("AUTOPILOT", new BitmapColor(255, 179, 0), new BitmapColor(76, 54, 0), PhysicalButton.B7);
    public static readonly PageTheme ComNav = new("COM / NAV", new BitmapColor(0, 204, 255), new BitmapColor(0, 61, 76), PhysicalButton.B8);

    public static readonly BitmapColor SoftkeyActive = new(0, 255, 101);
    public static readonly BitmapColor SoftkeyInactive = new(26, 26, 26);
    public static readonly BitmapColor SoftkeyText = BitmapColor.White;
    public static readonly BitmapColor ApOn = new(0, 255, 101);
    public static readonly BitmapColor ApArm = new(255, 179, 0);
    public static readonly BitmapColor ApOff = new(26, 26, 26);
    public static readonly BitmapColor PowerOff = BitmapColor.Black;

    public static IReadOnlyList<PageTheme> AllPages { get; } = [Pfd, Mfd, Autopilot, ComNav];
}
