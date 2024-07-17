using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ChacharsTabBase : ComponentBase, IEntityTab
{
    private string _htmlTitle = $"{StringConstants.ASCII_PINYIN} - {StringConstants.CHARACTERS}";

    protected ChacharForm ChacharForm { get; set; } = default!;

    protected ChacharViewDialog ChacharViewDialog { get; set; } = default!;

    public string ButtonId { get; } = IDs.CHACHARS_TAB_BUTTON;

    public bool IsVisible { get; private set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IIndex Index { get; set; } = default!;

    protected override void OnInitialized() =>
        _htmlTitle = $"{StringConstants.ASCII_PINYIN} - {Localizer[Resource.Characters]}";

    public async Task HideAsync(CancellationToken cancellationToken)
    {
        IsVisible = false;
        await JSInteropDOM.HideElementAsync(IDs.NAVBAR_CHACHARS_TAB_ROOT, cancellationToken);
    }

    public async Task ShowAsync(CancellationToken cancellationToken)
    {
        IsVisible = true;
        await JSInteropDOM.SetTitleAsync(_htmlTitle, cancellationToken);
        await JSInteropDOM.ShowElementAsync(IDs.NAVBAR_CHACHARS_TAB_ROOT, cancellationToken);
    }

    protected async Task ShowChacharFormAsync(CancellationToken cancellationToken) =>
        await ChacharForm.OpenAsync(_htmlTitle, cancellationToken);

    protected async Task ShowChacharViewDialogAsync(Chachar chachar, CancellationToken cancellationToken) =>
        await ChacharViewDialog.OpenAsync(chachar, _htmlTitle, cancellationToken);
}
