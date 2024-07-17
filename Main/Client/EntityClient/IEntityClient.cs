using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.EntityClient;

public interface IEntityClient
{
    Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
        string entitiesApiName,
        CancellationToken cancellationToken
    ) where TEntity : IEntity;

    Task<HttpStatusCode> PostEntityAsync<TEntity>(
        string entitiesApiName,
        TEntity entity,
        CancellationToken cancellationToken
    ) where TEntity : IEntity;
}
