using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class EntityCardBase<TEntity> : ComponentBase where TEntity : IEntity
{
    [Parameter]
    public char Body { get; set; } = default!;

    [Parameter]
    public string Footer { get; set; } = default!;

    [Parameter]
    public Action<TEntity> SelectEntity { get; set; } = default!;

    [Parameter]
    public TEntity SelectEntityArgument { get; set; } = default!;
}
