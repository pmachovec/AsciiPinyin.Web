using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ModalWithBackdropCommons(IJSInteropDOM _jSInteropDOM) : IModalWithBackdropCommons
{
    public async Task OpenAsyncCommon(
        ModalWithBackdropBase modalComponent,
        string htmlTitle,
        CancellationToken cancellationToken)
    {
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.D_NONE, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.D_BLOCK, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.SHOW, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken);
    }

    public async Task CloseAsyncCommon(
        ModalWithBackdropBase modalComponent,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken)
    {
        EventOnClose?.Invoke(modalComponent, EventArgs.Empty);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.SHOW, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.D_BLOCK, cancellationToken);
        await _jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.D_NONE, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken);
    }
}
