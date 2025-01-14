using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Shared.Components;

public class NavBarBase : ComponentBase
{
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IEntityTab AlternativesTab { get; init; }

    [Parameter]
    public required IEntityTab ChacharsTab { get; init; }

    [Parameter]
    public required Func<IEntityTab, CancellationToken, Task> SelectTabAsync { get; init; }
}
