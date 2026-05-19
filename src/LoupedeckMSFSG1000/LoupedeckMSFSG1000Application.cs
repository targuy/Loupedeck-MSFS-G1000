namespace LoupedeckMSFSG1000;

using Loupedeck;

public class LoupedeckMSFSG1000Application : ClientApplication
{
    protected override String GetProcessName() => "";

    protected override String GetBundleName() => "";

    public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
}
