using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Components;

public class EntitySelectorBase<TEntity> : ComponentBase, IEntityFormModal where TEntity : IEntity
{
    public IEntityForm EntityForm { get; private set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public string RootId { get; set; } = default!;

    [Parameter]
    public IEnumerable<TEntity> Entities { get; set; } = default!;

    [Parameter]
    public string HtmlTitle { private get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public Func<TEntity, CancellationToken, Task> SelectEntityAsync { get; set; } = default!;

    public async Task OpenAsync(
        IEntityForm entityForm,
        CancellationToken cancellationToken
    )
    {
        EntityForm = entityForm;

        await ModalCommons.OpenAsyncCommon(
            this,
            HtmlTitle,
            cancellationToken
        );
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
}
