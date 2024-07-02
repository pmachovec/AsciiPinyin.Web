using AsciiPinyin.Web.Client.AbstractComponentBases;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharViewDialogBase : ModalBaseEntitySpecific<Chachar>
{
    protected Chachar? Chachar { get; set; }

    public override string RootId { get; } = IDs.CHACHAR_VIEW_DIALOG_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalWithBackdropCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public override async Task OpenAsync(Chachar entity, CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);

        Chachar = entity;
        StateHasChanged();
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);

        Chachar = null;
        StateHasChanged();
    }
}
