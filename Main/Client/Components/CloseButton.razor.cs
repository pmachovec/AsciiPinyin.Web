using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Components;

public class CloseButtonBase : ComponentBase
{
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; init; } = default!;

    [Parameter, EditorRequired]
    public required Func<CancellationToken, Task> CloseAsync { get; init; }

    [Parameter, EditorRequired]
    public required CancellationToken CancellationToken { get; init; }
}
