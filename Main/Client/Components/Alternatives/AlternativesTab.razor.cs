using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Alternatives;

public class AlternativesTabBase : ComponentBase, ITab
{
    public bool IsVisible { get; set; } = false;
    public string Title { get; private set; } = StringConstants.ALTERNATIVES;

    [Parameter]
    public Alternative[]? Alternatives { get; set; }

#pragma warning disable CS8618
    protected AlternativeViewDialog alternativeViewDialog;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; }

    [Inject]
    private ILokal Lokal { get; set; }
#pragma warning restore CS8618

    protected override void OnInitialized()
    {
        Title = $"{Lokal.AsciiPinyin} - {Lokal.Alternatives}";
    }

    protected async void SelectAlternative(Alternative alternative)
    {
        await alternativeViewDialog.SetAlternative(alternative);
    }
}
