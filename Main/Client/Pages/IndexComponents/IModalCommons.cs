namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IModalCommons
{
    Task OpenAsyncCommon(
        ModalBase modalComponent,
        string htmlTitle,
        CancellationToken cancellationToken);

    Task CloseAsyncCommon(
        ModalBase modalComponent,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken);
}
