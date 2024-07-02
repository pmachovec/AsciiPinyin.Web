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
    public IEntityTab AlternativesTab { get; set; } = default!;

    [Parameter]
    public IEntityTab ChacharsTab { get; set; } = default!;

    [Parameter]
    public Func<IEntityTab, CancellationToken, Task> SelectTabAsync { get; set; } = (_, _) => Task.CompletedTask;
}
