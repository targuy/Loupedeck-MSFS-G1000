using LoupedeckMSFSG1000.Common;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.G1000.Pages;

namespace LoupedeckMSFSG1000.Adapters;

public class CTAdapter : IDeviceAdapter
{
    public void SetButtonLedColor(PhysicalButton button, BitmapColor color) => throw new NotImplementedException();

    public void SetAllButtonsBrightness(int percent) => throw new NotImplementedException();

    public void OnPageButtonPressed(Action<G1000Page> handler) => throw new NotImplementedException();

    public void Initialize(G1000Folder folder) => throw new NotImplementedException();

    public void OnWheelRight(int delta) => throw new NotImplementedException();

    public void OnWheelLeft(int delta) => throw new NotImplementedException();

    public void RefreshWheelScreen() => throw new NotImplementedException();
}
