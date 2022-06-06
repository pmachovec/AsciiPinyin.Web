using AsciiPinyin.Web.Client.Shared;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class ChacharListBase : ComponentBase
{
    [Parameter]
    public Chachar[]? Chachars { protected get; set; }

#pragma warning disable CS8618
    protected ChacharViewDialog chacharViewDialog;

    [Inject]
    protected JSInteropConsole JSInteropConsole { get; set; }
#pragma warning restore CS8618

    protected async void SelectChachar(Chachar chachar)
    {
        await chacharViewDialog.SetChachar(chachar);
    }
}
