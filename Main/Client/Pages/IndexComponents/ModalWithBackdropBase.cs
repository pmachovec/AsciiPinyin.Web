using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public abstract class ModalWithBackdropBase : ComponentBase
{
    public abstract string BackdropId { get; }

    public abstract string RootId { get; }

    public abstract event EventHandler EventOnClose;

    public abstract Task CloseAsync(CancellationToken cancellationToken);
}
