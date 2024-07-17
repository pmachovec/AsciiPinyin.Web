using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class ModalCommons(IJSInteropDOM _jSInteropDOM) : IModalCommons
{
    public async Task OpenAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(modalFirstLevel.BackdropId, CssClasses.D_NONE, cancellationToken),
            _jSInteropDOM.AddClassAsync(modalFirstLevel.BackdropId, CssClasses.D_BLOCK, cancellationToken)
        );

        await OpenAsyncCommonCommon(modalFirstLevel, htmlTitle, htmlTitleOnClose, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalFirstLevel.BackdropId, CssClasses.SHOW, cancellationToken);
    }

    public async Task OpenAsyncCommon(
        IModalSecondLevel modalSecondLevel,
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    )
    {
        modalSecondLevel.ModalFirstLevel = modalFirstLevel;

        await Task.WhenAll(
            _jSInteropDOM.SetZIndexAsync(
                modalSecondLevel.ModalFirstLevel.RootId,
                ByteConstants.INDEX_BACKDROP_Z - 1,
                cancellationToken
            ),
            OpenAsyncCommonCommon(modalSecondLevel, htmlTitle, htmlTitleOnClose, cancellationToken)
        );
    }

    public async Task CloseAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.RemoveClassAsync(modalFirstLevel.BackdropId, CssClasses.SHOW, cancellationToken);
        await CloseAsyncCommonCommon(modalFirstLevel, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(modalFirstLevel.BackdropId, CssClasses.D_BLOCK, cancellationToken),
            _jSInteropDOM.AddClassAsync(modalFirstLevel.BackdropId, CssClasses.D_NONE, cancellationToken)
        );
    }

    public async Task CloseAsyncCommon(
        IModalSecondLevel modalSecondLevel,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.SetZIndexAsync(
                modalSecondLevel.ModalFirstLevel.RootId,
                ByteConstants.INDEX_BACKDROP_Z + 1,
                cancellationToken
            ),
            CloseAsyncCommonCommon(modalSecondLevel, cancellationToken)
        );
    }

    private async Task OpenAsyncCommonCommon(
        IModal modal,
        string htmlTitle,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    )
    {
        modal.HtmlTitleOnClose = htmlTitleOnClose;

        await Task.WhenAll(
            _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(modal.RootId, CssClasses.D_NONE, cancellationToken),
            _jSInteropDOM.AddClassAsync(modal.RootId, CssClasses.D_BLOCK, cancellationToken)
        );

        // This separation and ordering is important because of the fade effect when opening the form.
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken);
    }

    private async Task CloseAsyncCommonCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.SetTitleAsync(modal.HtmlTitleOnClose, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken)
        );

        // This separation and ordering is important because of the fade effect when closing the form.
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(modal.RootId, CssClasses.D_BLOCK, cancellationToken),
            _jSInteropDOM.AddClassAsync(modal.RootId, CssClasses.D_NONE, cancellationToken)
        );
    }
}
