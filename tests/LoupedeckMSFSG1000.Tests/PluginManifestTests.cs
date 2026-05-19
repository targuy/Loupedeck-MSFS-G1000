namespace LoupedeckMSFSG1000.Tests;

public class PluginManifestTests
{
    [Fact]
    public void Name_ShouldMatchExpectedPluginName()
    {
        Assert.Equal("Loupedeck MSFS G1000", PluginManifest.Name);
    }
}
