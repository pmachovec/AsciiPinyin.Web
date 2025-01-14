using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class EntityCardsBase<T> : ComponentBase where T : IEntity
{
    [Parameter]
    public required IEnumerable<T> Entities { get; set; } = default!;

    [Parameter]
    public required Func<T, CancellationToken, Task> SelectEntityAsync { get; set; } = default!;
}
