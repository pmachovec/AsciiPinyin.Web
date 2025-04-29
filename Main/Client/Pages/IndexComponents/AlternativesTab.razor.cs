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
    protected AlternativeForm AlternativeForm { get; set; } = default!;

    protected AlternativeViewDialog AlternativeViewDialog { get; set; } = default!;

    public string Classes { get; set; } = string.Empty;

    public string HtmlTitle { get; private set; } = string.Empty;

    public bool IsVisible { get; private set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected ILogger<AlternativesTab> Logger { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required IIndex Index { get; init; }

    protected override void OnInitialized() =>
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {Localizer[Resource.Alternatives]}";

    public async Task HideAsync(CancellationToken cancellationToken)
    {
        IsVisible = false;
        await JSInteropDOM.HideElementAsync(IDs.NAVBAR_ALTERNATIVES_TAB_ROOT, cancellationToken);
    }

    public async Task ShowAsync(CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        IsVisible = true;
        await JSInteropDOM.ShowElementAsync(IDs.NAVBAR_ALTERNATIVES_TAB_ROOT, cancellationToken);
    }

    protected async Task ShowAlternativeFormAsync(CancellationToken cancellationToken) =>
        await AlternativeForm.OpenAsync(Index, cancellationToken);

    public async Task ShowAlternativeViewDialogAsync(Alternative alternative, CancellationToken cancellationToken) =>
        await AlternativeViewDialog.OpenAsync(Index, alternative, cancellationToken);
}
