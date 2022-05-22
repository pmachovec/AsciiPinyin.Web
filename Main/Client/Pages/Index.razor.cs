using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase: ComponentBase
{
    protected Type SelectedTabType { get; private set; } = typeof(ChacharList);
    protected Chachar[]? Chachars { get; private set; }
    // protected Alternative[]? Alternatives { get; private set; }

    [Inject]
    private HttpClient? HttpClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (HttpClient == null)
        {
            Console.Error.WriteLine("IndexBase.OnInitializedAsync: HttpClient is null");
        }
        else
        {
            Chachars = await LoadEntitiesAsync<Chachar>(HttpClient, "characters");
            // Alternatives = await  LoadEntitiesAsync<Alternative>(HttpClient, "alternatives");
        }
    }

    protected void SelectTab(Type tabType)
    {
        SelectedTabType = tabType;
    }

    protected string GetActiveIfActive(Type tabType)
    {
        return SelectedTabType.Equals(tabType) ? "active" : "";
    }

    private static async Task<T[]?> LoadEntitiesAsync<T>(HttpClient httpClient, string entitiesApiName) where T : IEntity
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T[]>(entitiesApiName);
        }
        catch
        {
            Console.Error.WriteLine($"IndexBase.LoadEntitiesAsync: Loading of {entitiesApiName} failed");
        }

        return null;
    }
}
