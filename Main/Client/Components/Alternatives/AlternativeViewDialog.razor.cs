using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Alternatives;

public class AlternativeViewDialogBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject]
    protected ILokal Lokal { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected Alternative? Alternative { get; set; }
    protected string ModalShow { get; set; } = "";
    protected string ModalDisplay { get; set; } = "d-none";

    public async Task SetAlternative(Alternative alternative)
    {
        JSInteropDOM.SetTitle($"{Lokal.AsciiPinyin} - {alternative.TheCharacter}");
        Alternative = alternative;
        ModalDisplay = "d-block";
        StateHasChanged();
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetAlternative()
    {
        JSInteropDOM.SetTitle($"{Lokal.AsciiPinyin} - {Lokal.Alternatives}");
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Alternative = null;
        StateHasChanged();
    }
}
