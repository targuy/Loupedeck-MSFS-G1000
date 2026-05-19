namespace LoupedeckMSFSG1000.Actions;

using LoupedeckMSFSG1000.G1000;

internal static class DisplayText
{
    public const String Hidden = " ";

    public static String WorkflowTitle(G1000ControlPage page) => FrameTitle(page switch
    {
        G1000ControlPage.Pfd => "PFD",
        G1000ControlPage.Mfd => "MFD",
        G1000ControlPage.Autopilot => "AP",
        G1000ControlPage.ComNav => "COM",
        _ => "SYS",
    });

    private static String FrameTitle(String title)
    {
        const Int32 targetLength = 9;
        const String block = "⬜";
        var blocks = Math.Max(2, targetLength - title.Length);
        var left = blocks / 2;
        var right = blocks - left;
        return $"{String.Concat(Enumerable.Repeat(block, left))}{title}{String.Concat(Enumerable.Repeat(block, right))}";
    }
}
