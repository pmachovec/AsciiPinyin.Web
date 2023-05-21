using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Lokal;
using AsciiPinyin.Web.Client.Pages.Index.Components;
using AsciiPinyin.Web.Client.Pages.Index.Components.Alternatives;
using AsciiPinyin.Web.Client.Pages.Index.Components.Chachars;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.Index;

public class IndexBase : ComponentBase
{
    private IEntityTab? _selectedTab;

    protected ChacharsTab chacharsTab = default!;
    protected AlternativesTab alternativesTab = default!;

    [Inject]
    protected ILokal Lokal { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            SelectTab(chacharsTab);
        }
    }

    protected void SelectTab(IEntityTab tab)
    {
        _selectedTab?.Hide();

        if (!tab.AreEntitiesInitialized)
        {
            tab.InitializeEntites();
        }

        tab.Show();
        _selectedTab = tab;
        JSInteropDOM.SetTitle(tab.Title);
        StateHasChanged();
    }

    protected static string GetActiveIfVisible(IEntityTab? tab) => (tab != null) && tab.IsVisible ? "active" : "";
}
