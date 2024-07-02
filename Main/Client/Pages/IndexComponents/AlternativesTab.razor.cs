using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class AlternativesTabBase : ComponentBase, IEntityTab
{
    private string _htmlTitle = $"{StringConstants.ASCII_PINYIN} - {StringConstants.ALTERNATIVES}";

    protected AlternativeForm AlternativeForm { get; set; } = default!;

    protected AlternativeViewDialog AlternativeViewDialog { get; set; } = default!;

    public string ButtonId { get; } = IDs.ALTERNATIVES_TAB_BUTTON;
    public bool IsVisible { get; private set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required Index Index { get; set; } = default!;

    protected override void OnInitialized() =>
        _htmlTitle = $"{StringConstants.ASCII_PINYIN} - {Localizer[Resource.Alternatives]}";

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            AlternativeForm.EventOnClose +=
                async (_, _) => await JSInteropDOM.SetTitleAsync(_htmlTitle, CancellationToken.None);
            AlternativeViewDialog.EventOnClose +=
                async (_, _) => await JSInteropDOM.SetTitleAsync(_htmlTitle, CancellationToken.None);
        }
    }

    public async Task HideAsync(CancellationToken cancellationToken)
    {
        IsVisible = false;
        await JSInteropDOM.HideElementAsync(IDs.ALTERNATIVES_TAB_ROOT, cancellationToken);
    }

    public async Task ShowAsync(CancellationToken cancellationToken)
    {
        IsVisible = true;
        await JSInteropDOM.SetTitleAsync(_htmlTitle, cancellationToken);
        await JSInteropDOM.ShowElementAsync(IDs.ALTERNATIVES_TAB_ROOT, cancellationToken);
    }

    protected async Task ShowAlternativeFormAsync(CancellationToken cancellationToken) =>
        await AlternativeForm.OpenAsync(cancellationToken);

    public async Task SelectAlternativeAsync(Alternative alternative, CancellationToken cancellationToken) =>
        await AlternativeViewDialog.OpenAsync(alternative, cancellationToken);
}
