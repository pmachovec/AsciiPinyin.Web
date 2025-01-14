using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class EntityCardsBase<T> : ComponentBase where T : IEntity
{
    [Parameter, EditorRequired]
    public required IEnumerable<T> Entities { get; init; }

    [Parameter, EditorRequired]
    public required Func<T, CancellationToken, Task> SelectEntityAsync { get; init; }

    [Parameter, EditorRequired]
    public required string SelectorClass { get; init; }
}
