using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Resources;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Chachars;

public class ChacharViewDialogBase : ComponentBase
{
    protected Chachar? Chachar { get; set; }
    protected string ModalShow { get; set; } = string.Empty;
    protected string ModalDisplay { get; set; } = "d-none";

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task SetChachar(Chachar chachar)
    {
        JSInteropDOM.SetTitle($"{Localizer[Resource.AsciiPinyin]} - {chachar.TheCharacter}");
        Chachar = chachar;
        ModalDisplay = "d-block";
        StateHasChanged(); // Must be also here, otherwise, the fade effect doesn't work.
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetChachar()
    {
        JSInteropDOM.SetTitle($"{Localizer[Resource.AsciiPinyin]} - {Localizer[Resource.Characters]}");
        ModalShow = string.Empty;
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Chachar = null;
        StateHasChanged();
    }
}
