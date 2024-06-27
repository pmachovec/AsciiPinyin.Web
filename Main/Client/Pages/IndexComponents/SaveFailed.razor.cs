using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class SaveFailedBase : ModalBaseGeneral
{
    public override string RootId { get; } = IDs.SAVE_FAILED_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalWithBackdropCommons { get; set; } = default!;

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            "Failed",
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
