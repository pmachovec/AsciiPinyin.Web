using AsciiPinyin.Web.Client.Shared;
using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharViewDialogBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject]
    private IJSRuntime JSRuntime { get; set; }

    [Inject]
    private SafeLocalization SafeLocalization { get; set; }
#pragma warning restore CS8618

    protected Chachar? Chachar { get; set; }
    protected string ModalShow { get; set; } = "";
    protected string ModalDisplay { get; set; } = "d-none";

    public async Task SetChachar(Chachar chachar)
    {
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} - {chachar.TheCharacter}", JSRuntime);
        Chachar = chachar;
        ModalDisplay = "d-block";
        StateHasChanged(); // Must be also here, otherwise, the fade effect doesn't work.
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetChachar()
    {
        JSInteropDOM.SetTitle($"{StringConstants.AsciiPinyin} - {GetLocalizedString("Characters")}", JSRuntime);
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Chachar = null;
        StateHasChanged();
    }

    protected string GetLocalizedString(string theString)
    {
        return SafeLocalization.GetLocalizedString(theString, "ChacharViewDialogBase");
    }
}
