using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents.AlternativesCardsComponents;

public class AlternativeViewDialogBase : ComponentBase
{
    protected Alternative? Alternative { get; set; }
    protected string ModalShow { get; set; } = string.Empty;
    protected string ModalDisplay { get; set; } = "d-none";

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task SetAlternative(Alternative alternative)
    {
        JSInteropDOM.SetTitle($"{Localizer[Resource.AsciiPinyin]} - {alternative.TheCharacter}");
        Alternative = alternative;
        ModalDisplay = "d-block";
        StateHasChanged();
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnsetAlternative()
    {
        JSInteropDOM.SetTitle($"{Localizer[Resource.AsciiPinyin]} - {Localizer[Resource.Alternatives]}");
        ModalShow = string.Empty;
        await Task.Delay(400);
        ModalDisplay = "d-none";
        Alternative = null;
        StateHasChanged();
    }
}
