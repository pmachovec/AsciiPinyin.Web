using AsciiPinyin.Web.Client.ComponentInterfaces;

namespace AsciiPinyin.Web.Client.Commons;

public interface IModalCommons
{
    Task OpenAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    );

    Task OpenAsyncCommon(
        IModalSecondLevel modalSecondLevel,
        IModalFirstLevel modalFirstLevel,
        string htmlTitle,
        string htmlTitleOnClose,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IModalFirstLevel modalFirstLevel,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IModalSecondLevel modalSecondLevel,
        CancellationToken cancellationToken
    );
}
