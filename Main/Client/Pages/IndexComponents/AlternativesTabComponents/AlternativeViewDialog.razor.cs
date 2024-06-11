using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ModalWithBackdropBaseSpecific<Alternative>
{
    protected Alternative? Alternative { get; set; }

    public override string BackdropId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_BACKDROP;

    public override string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IModalWithBackdropCommons ModalWithBackdropCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public override async Task OpenAsync(Alternative entity, CancellationToken cancellationToken)
    {
        Alternative = entity;
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);
        StateHasChanged();
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        Alternative = null;
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);
        StateHasChanged();
    }
}
