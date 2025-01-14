using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class NewEntityCardBase : ComponentBase
{
    [Parameter, EditorRequired]
    public required string Label { get; init; }

    [Parameter, EditorRequired]
    public required Func<CancellationToken, Task> ShowEntityFormAsync { get; init; }
}
