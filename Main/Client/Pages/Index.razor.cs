using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.Dependencies;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase
{
    protected Type SelectedTabType { get; private set; } = typeof(ChacharList);
    protected Chachar[]? Chachars { get; private set; }
    // protected Alternative[]? Alternatives { get; private set; }

#pragma warning disable CS8618
    [Inject]
    protected ISafeLocalizer SafeLocalizer { get; set; }

    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected override async Task OnInitializedAsync()
    {
        Chachars = await LoadEntitiesAsync<Chachar>(HttpClient, "characters");
        // Alternatives = await  LoadEntitiesAsync<Alternative>(HttpClient, "alternatives");
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} -  {SafeLocalizer.GetString("Characters")}");
    }

    protected void SelectTabWithLocalizedTitle(Type tabType, string titleLocalizedPartKey)
    {
        SelectedTabType = tabType;
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} -  {SafeLocalizer.GetString(titleLocalizedPartKey)}");
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
