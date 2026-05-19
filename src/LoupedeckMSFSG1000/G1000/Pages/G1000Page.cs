using LoupedeckMSFSG1000.Common;

namespace LoupedeckMSFSG1000.G1000.Pages;

public abstract class G1000Page
{
    protected G1000Page(string id, string name, PageTheme theme)
    {
        Id = id;
        Name = name;
        Theme = theme;
    }

    public string Id { get; }
    public string Name { get; }
    public PageTheme Theme { get; }

    public virtual int RepresentationCount => 1;
}
