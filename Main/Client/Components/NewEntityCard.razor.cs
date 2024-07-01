using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class NewEntityCardBase : ComponentBase
{
    [Parameter]
    public required string Label { get; set; } = default!;

    [Parameter]
    public required Func<CancellationToken, Task> ShowEntityFormAsync { get; set; } = default!;
}
