using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharListBase: ComponentBase
{
    [Parameter]
    public Chachar[]? Chachars { protected get; set; }

    protected ChacharViewDialog? chacharViewDialog;

    protected async void SelectChachar(Chachar chachar)
    {
        if (chacharViewDialog == null)
        {
            Console.Error.WriteLine($"ChacharListBase.SelectChachar: Chachar view dialog is null.");
        }
        else
        {
            await chacharViewDialog.SetChachar(chachar);
        }
    }
}
