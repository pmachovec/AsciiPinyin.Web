using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharViewDialogBase : ModalWithBackdropBaseSpecific<Chachar>
{
    protected Chachar? Chachar { get; set; }

    public override string BackdropId { get; } = IDs.CHACHAR_VIEW_DIALOG_BACKDROP;

    public override string RootId { get; } = IDs.CHACHAR_VIEW_DIALOG_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public override async Task OpenAsync(Chachar entity, CancellationToken cancellationToken)
    {
        Chachar = entity;
        await this.OpenAsyncExtension(
            JSInteropDOM,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);
        StateHasChanged();
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        Chachar = null;
        await this.CloseAsyncExtension(
            JSInteropDOM,
            EventOnClose,
            cancellationToken);
        StateHasChanged();
    }
}
