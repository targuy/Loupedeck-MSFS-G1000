namespace LoupedeckMSFSG1000.G1000.Controls;

public class StateButton
{
    public required string Id { get; init; }
    public bool IsOn { get; private set; }

    public void SetState(bool isOn)
    {
        IsOn = isOn;
        throw new NotImplementedException();
    }
}
