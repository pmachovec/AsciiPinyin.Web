using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Components;

public class EntitySelectorBase<T> : ComponentBase, IModal where T : IEntity
{
    protected string Classes { get; private set; } = CssClasses.D_NONE;

    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required string RootId { get; init; }

    [Parameter, EditorRequired]
    public required IEnumerable<T> Entities { get; init; }

    [Parameter, EditorRequired]
    public required string HtmlTitle { get; init; }

    [Parameter, EditorRequired]
    public required string Title { get; init; }

    [Parameter, EditorRequired]
    public required string SelectorClass { get; init; }

    [Parameter, EditorRequired]
    public required Func<T, CancellationToken, Task> SelectEntityAsync { get; init; }

    public async Task OpenAsync(
        IModal modalLowerLevel,
        CancellationToken cancellationToken
    )
    {
        ModalLowerLevel = modalLowerLevel;

        await ModalCommons.OpenAsyncCommon(
            this,
            HtmlTitle,
            cancellationToken
        );
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);
}
