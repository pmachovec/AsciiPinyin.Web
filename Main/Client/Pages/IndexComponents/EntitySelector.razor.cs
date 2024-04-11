using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class EntitySelectorBase<TEntity> : ComponentBase where TEntity : IEntity
{
    public event EventHandler EventOnClose = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public IEnumerable<TEntity> Entities { get; set; } = default!;

    [Parameter]
    public string HtmlTitle { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public string RootId { get; set; } = default!;

    [Parameter]
    public Func<TEntity, CancellationToken, Task> SelectEntityAsync { get; set; } = default!;

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        await JSInteropDOM.RemoveClassAsync(RootId, CssClasses.D_NONE, cancellationToken);
        await JSInteropDOM.AddClassAsync(RootId, CssClasses.D_BLOCK, cancellationToken);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await JSInteropDOM.AddClassAsync(RootId, CssClasses.SHOW, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        EventOnClose?.Invoke(this, EventArgs.Empty);
        await JSInteropDOM.RemoveClassAsync(RootId, CssClasses.SHOW, cancellationToken);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await JSInteropDOM.RemoveClassAsync(RootId, CssClasses.D_BLOCK, cancellationToken);
        await JSInteropDOM.AddClassAsync(RootId, CssClasses.D_NONE, cancellationToken);
    }
}
