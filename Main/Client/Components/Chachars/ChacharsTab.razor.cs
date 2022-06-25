using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Chachars;

public class ChacharsTabBase : ComponentBase, ITab
{
    public bool IsVisible { get; set; } = false;
    public string Title { get; private set; } = StringConstants.CHARACTERS;

    [Parameter]
    public Chachar[]? Chachars { protected get; set; }

#pragma warning disable CS8618
    protected ChacharViewDialog chacharViewDialog;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; }

    [Inject]
    private ILokal Lokal { get; set; }
#pragma warning restore CS8618

    protected override void OnInitialized()
    {
        Title = $"{Lokal.AsciiPinyin} - {Lokal.Characters}";
    }

    protected async void SelectChachar(Chachar chachar)
    {
        await chacharViewDialog.SetChachar(chachar);
    }
}
