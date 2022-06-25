using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.Components.Chachars;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase
{
    protected Type SelectedTabType { get; private set; } = typeof(ChacharsTab);
    protected Chachar[]? Chachars { get; private set; }
    protected Alternative[]? Alternatives { get; private set; }
    private ITab? _selectedTab;

#pragma warning disable CS8618
    protected ITab chacharsTab;
    protected ITab alternativesTab;

    [Inject]
    protected ILokal Lokal { get; set; }

    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    private IJSInteropConsole JSInteropConsole { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected override async Task OnInitializedAsync()
    {
        Chachars = await LoadEntitiesAsync<Chachar>("characters");
        Alternatives = await LoadEntitiesAsync<Alternative>("alternatives");
        SelectTab(chacharsTab);
    }

    protected void SelectTab(ITab tab)
    {
        if (_selectedTab != null)
        {
            _selectedTab.IsVisible = false;
        }

        tab.IsVisible = true;
        _selectedTab = tab;
        JSInteropDOM.SetTitle(tab.Title);
    }

    protected string GetActiveIfActive(ITab tab)
    {
        return (_selectedTab != null) && _selectedTab.GetType().Equals(tab.GetType()) ? "active" : "";
    }

    private async Task<T[]?> LoadEntitiesAsync<T>(string entitiesApiName) where T : IEntity
    {
        try
        {
            return await HttpClient.GetFromJsonAsync<T[]>(entitiesApiName);
        }
        catch
        {
            JSInteropConsole.ConsoleWarning($"IndexBase.LoadEntitiesAsync: Loading of {entitiesApiName} failed");
        }

        return null;
    }
}
