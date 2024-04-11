using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
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
        _htmlTitle = $"{StringConstants.ASCII_PINYIN} - {Localizer[Resource.Characters]}";

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            ChacharForm.EventOnClose +=
                async (_, _) => await JSInteropDOM.SetTitleAsync(_htmlTitle, CancellationToken.None);
            ChacharViewDialog.EventOnClose +=
                async (_, _) => await JSInteropDOM.SetTitleAsync(_htmlTitle, CancellationToken.None);
        }
    }

    public async Task HideAsync(CancellationToken cancellationToken)
    {
        IsVisible = false;
        await JSInteropDOM.HideElementAsync(IDs.CHACHARS_TAB_ROOT, cancellationToken);
    }

    public async Task ShowAsync(CancellationToken cancellationToken)
    {
        IsVisible = true;
        await JSInteropDOM.SetTitleAsync(_htmlTitle, cancellationToken);
        await JSInteropDOM.ShowElementAsync(IDs.CHACHARS_TAB_ROOT, cancellationToken);
    }

    protected async Task ShowChacharFormAsync(CancellationToken cancellationToken) =>
        await ChacharForm.OpenAsync(cancellationToken);

    protected async Task SelectChacharAsync(Chachar chachar, CancellationToken cancellationToken) =>
        await ChacharViewDialog.OpenAsync(chachar, cancellationToken);
}
