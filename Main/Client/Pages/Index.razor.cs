using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase: ComponentBase
{
    protected string SelectedTabName { get; private set; } = "ChacharList";

    protected void SelectTab(string tabName)
    {
        SelectedTabName = tabName;
    }

    protected string GetActiveIfActive(string tabName)
    {
        return SelectedTabName == tabName ? "active" : "";
    }
}
