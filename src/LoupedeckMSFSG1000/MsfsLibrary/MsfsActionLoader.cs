namespace LoupedeckMSFSG1000.MsfsLibrary;

public class MsfsActionLoader
{
    public Task<MsfsActionDefinition> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class MsfsActionDefinition
{
    public string Version { get; set; } = "1.0";
}
