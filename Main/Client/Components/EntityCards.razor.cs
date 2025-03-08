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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    protected string GetFooterText(T entity)
    {
        if (entity is Chachar chachar)
        {
            return chachar.RealPinyin ?? string.Empty;
        }
        else if (entity is Alternative alternative)
        {
            return alternative.OriginalCharacter ?? string.Empty;
        }
        else
        {
            throw new InvalidOperationException($"Invalid entity type '{entity.GetType()}'");
        }
    }
}
