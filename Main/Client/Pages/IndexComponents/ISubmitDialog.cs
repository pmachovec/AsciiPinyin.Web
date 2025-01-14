using AsciiPinyin.Web.Client.ComponentInterfaces;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface ISubmitDialog : IModal
{
    Task SetProcessingAsync(IEntityForm entityForm, CancellationToken cancellationToken);

    Task SetSuccessAsync(IModal modalLowerLevel, string message, CancellationToken cancellationToken);

    Task SetErrorAsync(IModal modalLowerLevel, string message, CancellationToken cancellationToken);

    Task SetWarningAsync(IModal modalLowerLevel, string message, Func<CancellationToken, Task> methodOnProceedAsync, CancellationToken cancellationToken);
}
