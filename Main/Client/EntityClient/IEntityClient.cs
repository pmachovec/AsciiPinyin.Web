using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.EntityClient;

public interface IEntityClient
{
    Task<IEnumerable<TEntity>> LoadEntitiesAsync<TEntity>(string entitiesApiName, CancellationToken cancellationToken) where TEntity : IEntity;
}
