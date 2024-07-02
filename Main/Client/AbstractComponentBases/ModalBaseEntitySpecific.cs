using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.AbstractComponentBases;

public abstract class ModalBaseEntitySpecific<TEntity> : ModalBase where TEntity : IEntity
{
    public abstract Task OpenAsync(TEntity entity, CancellationToken cancellationToken);
}
