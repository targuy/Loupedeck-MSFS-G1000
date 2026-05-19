using LoupedeckMSFSG1000.G1000.Pages;

namespace LoupedeckMSFSG1000.G1000;

public class G1000Folder
{
    public IReadOnlyList<G1000Page> Pages { get; init; } = [];

    public G1000Page? CurrentPage { get; private set; }

    public void NavigateToPage(G1000Page page)
    {
        CurrentPage = page;
        throw new NotImplementedException();
    }

    public void RefreshCurrentPage()
    {
        throw new NotImplementedException();
    }

    public void SetAllButtonsBlack()
    {
        throw new NotImplementedException();
    }
}
