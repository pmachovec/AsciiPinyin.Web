using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.Components.Alternatives;
using AsciiPinyin.Web.Client.Components.Chachars;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase
{
    private IEntityTab? _selectedTab;

#pragma warning disable CS8618
    protected ChacharsTab chacharsTab;
    protected AlternativesTab alternativesTab;

    [Inject]
    protected ILokal Lokal { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            SelectTab(chacharsTab);
        }
    }

    protected void SelectTab(IEntityTab tab)
    {
        if (_selectedTab != null)
        {
            _selectedTab.Hide();
        }

        if (!tab.AreEntitiesInitialized)
        {
            tab.InitializeEntites();
        }

        tab.Show();
        _selectedTab = tab;
        JSInteropDOM.SetTitle(tab.Title);
        StateHasChanged();
    }

    protected static string GetActiveIfVisible(IEntityTab? tab)
    {
        return (tab != null) && tab.IsVisible ? "active" : "";
    }
}
