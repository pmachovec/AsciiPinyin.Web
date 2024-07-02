using AsciiPinyin.Web.Client.AbstractComponentBases;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class SaveSuccessBase : ModalBaseGeneral
{
    public override string RootId { get; } = IDs.SAVE_SUCCESS_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalWithBackdropCommons { get; set; } = default!;

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            "Success",
            cancellationToken);
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);
    }
}
