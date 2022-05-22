using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharViewDialogBase: ComponentBase
{
    protected Chachar? Chachar { get; set; }
    protected string ModalShow { get; set; } = "";
    protected string ModalDisplay { get; set; } = "d-none";

    public async Task SetChachar(Chachar chachar)
    {
        Chachar = chachar;
        ModalDisplay = "d-block";
        StateHasChanged(); // Must be also here, otherwise, the fade effect doesn't work.
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetChachar()
    {
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Chachar = null;
        StateHasChanged();
    }
}
