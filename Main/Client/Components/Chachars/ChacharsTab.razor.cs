using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Chachars;

public class ChacharsTabBase : ComponentBase
{
    [Parameter]
    public Chachar[]? Chachars { protected get; set; }

#pragma warning disable CS8618
    protected ChacharViewDialog chacharViewDialog;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; }
#pragma warning restore CS8618

        protected async void SelectChachar(Chachar chachar)
    {
        await chacharViewDialog.SetChachar(chachar);
    }
}
