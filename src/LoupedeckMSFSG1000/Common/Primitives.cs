namespace LoupedeckMSFSG1000.Common;

public enum PhysicalButton
{
    B1,
    B2,
    B3,
    B4,
    B5,
    B6,
    B7,
    B8
}

public readonly record struct BitmapColor(byte R, byte G, byte B)
{
    public static BitmapColor White => new(255, 255, 255);
    public static BitmapColor Black => new(0, 0, 0);
}

public readonly record struct PageTheme(string Name, BitmapColor Color, BitmapColor ColorDim, PhysicalButton ButtonId);
