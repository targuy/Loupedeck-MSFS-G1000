namespace LoupedeckMSFSG1000.G1000;

using Loupedeck;

public sealed record PageTheme(
    G1000PageId Id,
    String Name,
    BitmapColor Color,
    BitmapColor ColorDim);
