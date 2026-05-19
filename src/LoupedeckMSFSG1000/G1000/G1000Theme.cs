namespace LoupedeckMSFSG1000.G1000;

using Loupedeck;

public static class G1000Theme
{
    public static readonly PageTheme Pfd = new(
        G1000PageId.Pfd,
        "PFD",
        new BitmapColor(0, 85, 255),
        new BitmapColor(0, 25, 76));

    public static readonly PageTheme Mfd = new(
        G1000PageId.Mfd,
        "MFD",
        new BitmapColor(0, 204, 68),
        new BitmapColor(0, 61, 20));

    public static readonly PageTheme Autopilot = new(
        G1000PageId.Autopilot,
        "AP",
        new BitmapColor(255, 179, 0),
        new BitmapColor(76, 54, 0));

    public static readonly PageTheme ComNav = new(
        G1000PageId.ComNav,
        "COM/NAV",
        new BitmapColor(0, 204, 255),
        new BitmapColor(0, 61, 76));

    public static IReadOnlyList<PageTheme> Pages { get; } =
    [
        Pfd,
        Mfd,
        Autopilot,
        ComNav,
    ];

    public static PageTheme GetTheme(G1000PageId pageId) =>
        Pages.First(page => page.Id == pageId);
}
