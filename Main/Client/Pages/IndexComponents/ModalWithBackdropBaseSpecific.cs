using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public abstract class ModalWithBackdropBaseSpecific<TEntity> : ModalWithBackdropBase where TEntity : IEntity
{
    public abstract Task OpenAsync(TEntity entity, CancellationToken cancellationToken);
}
