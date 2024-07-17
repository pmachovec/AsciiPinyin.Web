using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class SaveFailedBase : ComponentBase, IModalSecondLevel
{
    public string RootId { get; } = IDs.SAVE_FAILED_ROOT;

    public string BackdropId { get; } = IDs.INDEX_BACKDROP;

    public string HtmlTitleOnClose { get; set; } = default!;

    public IModalFirstLevel ModalFirstLevel { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    public async Task OpenAsync(
        IModalFirstLevel formModal,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    )
    {
        await ModalCommons.OpenAsyncCommon(
            this,
            formModal,
            "Failed",
            htmlTitleOnClose,
            cancellationToken
        );
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
}
