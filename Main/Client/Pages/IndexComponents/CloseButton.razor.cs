using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class CloseButtonBase : ComponentBase
{
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required Func<CancellationToken, Task> CloseAsync { get; set; } = default!;

    [Parameter]
    public required CancellationToken CancellationToken { get; set; } = default!;
}
