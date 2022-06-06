using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Alternatives;

public class AlternativesTabBase : ComponentBase
{
    [Parameter]
    public Alternative[]? Alternatives { get; set; }

#pragma warning disable CS8618
    protected AlternativeViewDialog alternativeViewDialog;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; }
#pragma warning restore CS8618



    protected async void SelectAlternative(Alternative alternative)
    {
        await alternativeViewDialog.SetAlternative(alternative);
    }
}
