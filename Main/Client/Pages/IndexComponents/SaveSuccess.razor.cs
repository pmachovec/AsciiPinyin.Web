using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class SaveSuccessBase : ComponentBase, IModalSecondLevel
{
    public string RootId { get; } = IDs.SAVE_SUCCESS_ROOT;

    public string BackdropId { get; } = IDs.INDEX_BACKDROP;

    public string HtmlTitleOnClose { get; set; } = default!;

    public IModalFirstLevel ModalFirstLevel { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    public async Task OpenAsync(
        IModalFirstLevel formModal,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    ) =>
        await ModalCommons.OpenAsyncCommon(
            this,
            formModal,
            "Success",
            htmlTitleOnClose,
            cancellationToken
        );

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            ModalCommons.CloseAsyncCommon(this, cancellationToken),
            ModalCommons.CloseAsyncCommon(ModalFirstLevel, cancellationToken)
        );
}
