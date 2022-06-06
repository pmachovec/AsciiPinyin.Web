using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.Dependencies;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharViewDialogBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject]
    protected ISafeLocalizer SafeLocalizer { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected Chachar? Chachar { get; set; }
    protected string ModalShow { get; set; } = "";
    protected string ModalDisplay { get; set; } = "d-none";

    public async Task SetChachar(Chachar chachar)
    {
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} - {chachar.TheCharacter}");
        Chachar = chachar;
        ModalDisplay = "d-block";
        StateHasChanged(); // Must be also here, otherwise, the fade effect doesn't work.
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetChachar()
    {
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} - {SafeLocalizer.GetString("Characters")}");
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Chachar = null;
        StateHasChanged();
    }
}
