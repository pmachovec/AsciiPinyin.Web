using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ModalBaseEntitySpecific<Alternative>
{
    protected Alternative? Alternative { get; set; }

    public override string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalWithBackdropCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public override async Task OpenAsync(Alternative entity, CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);

        Alternative = entity;
        StateHasChanged();
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);

        Alternative = null;
        StateHasChanged();
    }
}
