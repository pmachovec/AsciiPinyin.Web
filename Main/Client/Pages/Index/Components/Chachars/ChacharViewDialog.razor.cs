using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Chachars;

public class ChacharViewDialogBase : ComponentBase
{
    [Inject]
    protected ILokal Lokal { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    protected Chachar? Chachar { get; set; }
    protected string ModalShow { get; set; } = "";
    protected string ModalDisplay { get; set; } = "d-none";

    public async Task SetChachar(Chachar chachar)
    {
        JSInteropDOM.SetTitle($"{Lokal.AsciiPinyin} - {chachar.TheCharacter}");
        Chachar = chachar;
        ModalDisplay = "d-block";
        StateHasChanged(); // Must be also here, otherwise, the fade effect doesn't work.
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetChachar()
    {
        JSInteropDOM.SetTitle($"{Lokal.AsciiPinyin} - {Lokal.Characters}");
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Chachar = null;
        StateHasChanged();
    }
}
