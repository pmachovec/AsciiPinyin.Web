namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModal
{
    abstract string RootId { get; }

    abstract event EventHandler EventOnClose;

    abstract Task CloseAsync(CancellationToken cancellationToken);
}
