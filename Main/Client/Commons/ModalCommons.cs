using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class ModalCommons(IJSInteropDOM _jSInteropDOM) : IModalCommons
{
    public async Task OpenAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await _jSInteropDOM.None2BlockAsync(modalFirstLevel.Index.BackdropId, cancellationToken);
        await OpenAsyncCommonCommon(modalFirstLevel, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalFirstLevel.Index.BackdropId, CssClasses.SHOW, cancellationToken);
    }

    public async Task OpenAsyncCommon(
        IEntityFormModal entityFormModal,
        string htmlTitle,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await OpenAsyncCommon(entityFormModal, cancellationToken);
    }

    public async Task OpenAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.SetZIndexAsync(
                entityFormModal.EntityForm.RootId,
                ByteConstants.INDEX_BACKDROP_Z - 1,
                cancellationToken
            ),
            OpenAsyncCommonCommon(entityFormModal, cancellationToken)
        );
    }

    public async Task CloseAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.SetTitleAsync(modalFirstLevel.Index.SelectedTab.HtmlTitle, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalFirstLevel.Index.BackdropId, CssClasses.SHOW, cancellationToken);
        await CloseAsyncCommonCommon(modalFirstLevel, cancellationToken);
        await _jSInteropDOM.Block2NoneAsync(modalFirstLevel.Index.BackdropId, cancellationToken);
    }

    public async Task CloseAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.SetTitleAsync(entityFormModal.EntityForm.HtmlTitle, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.SetZIndexAsync(
                entityFormModal.EntityForm.RootId,
                ByteConstants.INDEX_BACKDROP_Z + 1,
                cancellationToken
            ),
            CloseAsyncCommonCommon(entityFormModal, cancellationToken)
        );
    }

    public async Task CloseAllAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    )
    {
        // First set the title to the current tab's title.
        await _jSInteropDOM.SetTitleAsync(entityFormModal.EntityForm.Index.SelectedTab.HtmlTitle, cancellationToken);

        // Then hide the first level modal.
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(entityFormModal.EntityForm.RootId, CssClasses.SHOW, cancellationToken),
            _jSInteropDOM.Block2NoneAsync(entityFormModal.EntityForm.RootId, cancellationToken)
        );

        // Then hide the second level modal and backdrop and move the first level modal before the backdrop.
        await _jSInteropDOM.RemoveClassAsync(entityFormModal.EntityForm.Index.BackdropId, CssClasses.SHOW, cancellationToken);
        await CloseAsyncCommonCommon(entityFormModal, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.Block2NoneAsync(entityFormModal.EntityForm.Index.BackdropId, cancellationToken),
            _jSInteropDOM.SetZIndexAsync(
                entityFormModal.EntityForm.RootId,
                ByteConstants.INDEX_BACKDROP_Z + 1,
                cancellationToken
            )
        );
    }

    private async Task OpenAsyncCommonCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.None2BlockAsync(modal.RootId, cancellationToken);

        // This separation and ordering is important because of the fade effect when opening the form.
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken);
    }

    private async Task CloseAsyncCommonCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        await _jSInteropDOM.RemoveClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken);

        // This separation and ordering is important because of the fade effect when closing the form.
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await _jSInteropDOM.Block2NoneAsync(modal.RootId, cancellationToken);
    }
}
