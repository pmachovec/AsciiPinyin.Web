using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModalEntitySpecific<TEntity> : IModal where TEntity : IEntity
{
    Task OpenAsync(TEntity entity, CancellationToken cancellationToken);
}
