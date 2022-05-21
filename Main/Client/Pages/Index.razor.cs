using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase: ComponentBase
{
    protected string SelectedTabName { get; private set; } = "ChacharList";
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
    }

    protected void SelectTab(string tabName)
    {
        SelectedTabName = tabName;
    }

    protected string GetActiveIfActive(string tabName)
    {
        return SelectedTabName == tabName ? "active" : "";
    }
}
