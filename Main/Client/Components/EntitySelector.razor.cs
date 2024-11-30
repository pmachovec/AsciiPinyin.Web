using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Components;

public class EntitySelectorBase<TEntity> : ComponentBase, IModal where TEntity : IEntity
{
    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public string RootId { get; set; } = default!;

    [Parameter]
    public IEnumerable<TEntity> Entities { get; set; } = default!;

    [Parameter]
    public string HtmlTitle { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public Func<TEntity, CancellationToken, Task> SelectEntityAsync { get; set; } = default!;

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
}
