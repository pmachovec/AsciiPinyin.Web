using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.EntityLoader;

public interface IEntityLoader
{
    Task<IEnumerable<TEntity>> LoadEntitiesAsync<TEntity>(string entitiesApiName, CancellationToken cancellationToken) where TEntity : IEntity;
}
