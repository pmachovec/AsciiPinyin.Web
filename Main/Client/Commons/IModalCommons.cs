
using AsciiPinyin.Web.Client.ComponentInterfaces;

namespace AsciiPinyin.Web.Client.Commons;

public interface IModalCommons
{
    Task OpenAsyncCommon(
        IModal modal,
        string htmlTitle,
        CancellationToken cancellationToken
    );

    Task OpenAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    );

    Task CloseAllAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    );
}
