using AsciiPinyin.Web.Client.ComponentInterfaces;

namespace AsciiPinyin.Web.Client.Commons;

public interface IModalCommons
{
    Task OpenAsyncCommon(
        IModal modalComponent,
        string htmlTitle,
        CancellationToken cancellationToken
    );

    Task CloseAsyncCommon(
        IModal modalComponent,
        EventHandler? EventOnClose,
        CancellationToken cancellationToken
    );
}
