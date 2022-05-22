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
        if (HttpClient != null)
        {
            Chachars = await HttpClient.GetFromJsonAsync<Chachar[]>("characters");
            // Alternatives = await HttpClient.GetFromJsonAsync<Alternative[]>("alternatives");
        }
        else
        {
            // TODO something
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
}
