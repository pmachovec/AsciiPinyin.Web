using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.Shared;
using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Resources;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase
{
    protected Type SelectedTabType { get; private set; } = typeof(ChacharList);
    protected Chachar[]? Chachars { get; private set; }
    // protected Alternative[]? Alternatives { get; private set; }

    #pragma warning disable CS8618
    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; }
    #pragma warning restore CS8618

    protected override async Task OnInitializedAsync()
    {
        Chachars = await LoadEntitiesAsync<Chachar>(HttpClient, "characters");
        // Alternatives = await  LoadEntitiesAsync<Alternative>(HttpClient, "alternatives");
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} -  {GetLocalizedString("Characters")}", JSRuntime);
    }

    protected void SelectTabWithLocalizedTitle(Type tabType, string titleLocalizedPartKey)
    {
        SelectedTabType = tabType;
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} -  {GetLocalizedString(titleLocalizedPartKey)}", JSRuntime);
    }

    protected string GetActiveIfActive(Type tabType)
    {
        return SelectedTabType.Equals(tabType) ? "active" : "";
    }

    protected string GetLocalizedString(string theString)
    {
        return SafeLocalization.GetLocalizedString(Localizer, theString, "IndexBase");
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
