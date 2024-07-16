using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class ModalCommons(IJSInteropDOM _jSInteropDOM) : IModalCommons
{
    public async Task OpenAsyncCommon(
        IModal modalComponent,
        string htmlTitle,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(IDs.INDEX_BACKDROP, CssClasses.D_NONE, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken),
            _jSInteropDOM.AddClassAsync(IDs.INDEX_BACKDROP, CssClasses.D_BLOCK, cancellationToken),
            _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken)
        );

        // This separation and ordering is important because of the fade effect when opening the form.
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.AddClassAsync(IDs.INDEX_BACKDROP, CssClasses.SHOW, cancellationToken),
            _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken)
        );
    }

    public async Task CloseAsyncCommon(
        IModal modalComponent,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken
    )
    {
        EventOnClose?.Invoke(modalComponent, EventArgs.Empty);
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(IDs.INDEX_BACKDROP, CssClasses.SHOW, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken)
        );

        // This separation and ordering is important because of the fade effect when closing the form.
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(IDs.INDEX_BACKDROP, CssClasses.D_BLOCK, cancellationToken),
            _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken),
            _jSInteropDOM.AddClassAsync(IDs.INDEX_BACKDROP, CssClasses.D_NONE, cancellationToken),
            _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken)
        );
    }
}
