using AsciiPinyin.Web.Client.AbstractComponentBases;

namespace AsciiPinyin.Web.Client.Commons;

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
