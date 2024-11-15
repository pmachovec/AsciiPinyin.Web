using AsciiPinyin.Web.Client.ComponentInterfaces;

namespace AsciiPinyin.Web.Client.Commons;

public interface IModalCommons
{
    Task OpenAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        CancellationToken cancellationToken
    );

    Task OpenAsyncCommon(
        IEntityFormModal entityFormModal,
        string htmlTitle,
        CancellationToken cancellationToken
    );

    Task OpenAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    );

    Task CloseAllAsyncCommon(
        IEntityFormModal entityFormModal,
        CancellationToken cancellationToken
    );
}
