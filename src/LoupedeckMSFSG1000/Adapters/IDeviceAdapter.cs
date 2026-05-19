using LoupedeckMSFSG1000.Common;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.G1000.Pages;

namespace LoupedeckMSFSG1000.Adapters;

public interface IDeviceAdapter
{
    void SetButtonLedColor(PhysicalButton button, BitmapColor color);
    void SetAllButtonsBrightness(int percent);
    void OnPageButtonPressed(Action<G1000Page> handler);
    void Initialize(G1000Folder folder);
}
