namespace LoupedeckMSFSG1000.Tests;

using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Msfs;

public sealed class ControlCatalogTests
{
    [Fact]
    public void G1000Commands_ShouldHaveUniqueIds()
    {
        AssertUnique(G1000ControlCatalog.Commands.Select(command => command.Id));
    }

    [Fact]
    public void G1000Adjustments_ShouldHaveUniqueIds()
    {
        AssertUnique(G1000ControlCatalog.Adjustments.Select(adjustment => adjustment.Id));
    }

    [Fact]
    public void MsfsCommands_ShouldHaveUniqueIds()
    {
        AssertUnique(MsfsControlCatalog.Commands.Select(command => command.Id));
    }

    [Fact]
    public void MsfsAdjustments_ShouldHaveUniqueIds()
    {
        AssertUnique(MsfsControlCatalog.Adjustments.Select(adjustment => adjustment.Id));
    }

    private static void AssertUnique(IEnumerable<String> ids)
    {
        var duplicate = ids
            .GroupBy(id => id)
            .FirstOrDefault(group => group.Count() > 1);
        Assert.Null(duplicate);
    }
}
