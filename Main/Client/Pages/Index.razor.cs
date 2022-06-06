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

#pragma warning disable CS8618
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
        Chachars = await LoadEntitiesAsync<Chachar>(HttpClient, "characters");
        Alternatives = await LoadEntitiesAsync<Alternative>(HttpClient, "alternatives");
        JSInteropDOM.SetTitle($"{Lokal.AsciiPinyin} -  {Lokal.Characters}");
    }

    protected void SelectTabWithPageTitleChange(Type tabType, string title)
    {
        SelectedTabType = tabType;
        JSInteropDOM.SetTitle(title);
    }

    protected string GetActiveIfActive(Type tabType)
    {
        return SelectedTabType.Equals(tabType) ? "active" : "";
    }

    private async Task<T[]?> LoadEntitiesAsync<T>(HttpClient httpClient, string entitiesApiName) where T : IEntity
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T[]>(entitiesApiName);
        }
        catch
        {
            JSInteropConsole.ConsoleWarning($"IndexBase.LoadEntitiesAsync: Loading of {entitiesApiName} failed");
        }

        return null;
    }
}
