using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class EntityCardsBase<TEntity> : ComponentBase where TEntity : IEntity
{
    [Parameter]
    public required IEnumerable<TEntity> Entities { get; set; } = default!;

    [Parameter]
    public required Func<TEntity, CancellationToken, Task> SelectEntityAsync { get; set; } = default!;
}
