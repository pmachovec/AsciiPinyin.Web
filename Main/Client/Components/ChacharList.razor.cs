using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharListBase: ComponentBase
{
    protected string ModalShow { get; private set; } = "";
    protected string ModalDisplay { get; private set; } = "d-none";
    protected Chachar? SelectedChachar { get; private set; }

    [Parameter]
    public Chachar[]? Chachars { protected get; set; }

    protected async void SelectChachar(char theCharacter, string pinyin)
    {
        if (Chachars == null)
        {
            // TODO something
            return;
        }

        SelectedChachar = Chachars.First(chachar => chachar.TheCharacter == theCharacter && chachar.Piniyin == pinyin);
        ModalDisplay = "d-block";
        await Task.Delay(10);
        ModalShow = "show";
        StateHasChanged();
    }

    protected async void UnselectChachar()
    {
        ModalShow = "";
        await Task.Delay(400);
        ModalDisplay = "d-none";
        SelectedChachar = null;
        StateHasChanged();
    }
}
