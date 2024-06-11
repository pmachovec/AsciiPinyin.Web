namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IModalWithBackdropCommons
{
    Task OpenAsyncCommon(
        ModalWithBackdropBase modalComponent,
        string htmlTitle,
        CancellationToken cancellationToken);

    Task CloseAsyncCommon(
        ModalWithBackdropBase modalComponent,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken);
}
