using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents.ChacharsCardsComponents;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharsCardsBase : ComponentBase
{
    [Parameter]
    public Chachar[] Chachars { get; set; } = default!;

    protected ChacharViewDialog ChacharViewDialog = default!;

    protected int NumberOfLines { get; set; } = 10;

    protected async void SelectChachar(Chachar chachar) => await ChacharViewDialog.SetChachar(chachar);

    protected static void ShowChacharForm()
    {
        //TODO
    }
}
