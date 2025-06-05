using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ChacharsTabBase : ComponentBase, IEntityTab
{
    public string Classes { get; set; } = string.Empty;

    public string HtmlTitle { get; private set; } = string.Empty;

    public bool IsVisible { get; private set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected ILogger<ChacharsTab> Logger { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required IIndex Index { get; init; }

    protected override void OnInitialized() =>
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {Localizer[Resource.Characters]}";

    public async Task HideAsync(CancellationToken cancellationToken)
    {
        IsVisible = false;
        await JSInteropDOM.HideElementAsync(IDs.NAVBAR_CHACHARS_TAB_ROOT, cancellationToken);
    }

    public async Task ShowAsync(CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        IsVisible = true;
        await JSInteropDOM.ShowElementAsync(IDs.NAVBAR_CHACHARS_TAB_ROOT, cancellationToken);
    }

    protected async Task ShowChacharFormAsync(CancellationToken cancellationToken) =>
        await Index.ChacharForm.OpenAsync(Index, cancellationToken);

    protected async Task ShowChacharViewDialogAsync(Chachar chachar, CancellationToken cancellationToken) =>
        await Index.ChacharViewDialog.OpenAsync(chachar, Index, cancellationToken);
}
