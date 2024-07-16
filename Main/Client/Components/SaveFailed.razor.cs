using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class SaveFailedBase : ComponentBase, IModalGeneral
{
    public string RootId { get; } = IDs.SAVE_FAILED_ROOT;

    public event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.OpenAsyncCommon(
            this,
            "Failed",
            cancellationToken
        );
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken
        );
    }
}
