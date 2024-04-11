using AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents.AlternativesCardsComponents;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativesCardsBase : ComponentBase
{
    [Parameter]
    public Alternative[] Alternatives { get; set; } = default!;

    protected AlternativeViewDialog AlternativeViewDialog = default!;

    protected int NumberOfLines { get; set; } = 10;

    public async void SelectAlternative(Alternative alternative) => await AlternativeViewDialog.SetAlternative(alternative);

    protected static void ShowAlternativeForm()
    {
        //TODO
    }
}
