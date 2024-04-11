using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public static class ModalWithBackdropExtension
{
    internal static async Task OpenAsyncExtension(
        this ModalWithBackdropBase modalComponent,
        IJSInteropDOM jSInteropDOM,
        string htmlTitle,
        CancellationToken cancellationToken)
    {
        await jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.D_NONE, cancellationToken);
        await jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.D_BLOCK, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.SHOW, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken);
    }

    internal static async Task CloseAsyncExtension(
        this ModalWithBackdropBase modalComponent,
        IJSInteropDOM jSInteropDOM,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken)
    {
        EventOnClose?.Invoke(modalComponent, EventArgs.Empty);
        await jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.SHOW, cancellationToken);
        await jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.SHOW, cancellationToken);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await jSInteropDOM.RemoveClassAsync(modalComponent.BackdropId, CssClasses.D_BLOCK, cancellationToken);
        await jSInteropDOM.RemoveClassAsync(modalComponent.RootId, CssClasses.D_BLOCK, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.BackdropId, CssClasses.D_NONE, cancellationToken);
        await jSInteropDOM.AddClassAsync(modalComponent.RootId, CssClasses.D_NONE, cancellationToken);
    }
}
