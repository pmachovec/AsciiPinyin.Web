using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.HttpClients;

public interface IEntityClient
{
    Task<ISet<TEntity>> GetEntitiesAsync<TEntity>(
        string entitiesApiName,
        CancellationToken cancellationToken
    ) where TEntity : IEntity;

    Task<HttpStatusCode> PostEntityAsync<TEntity>(
        string entitiesApiName,
        TEntity entity,
        CancellationToken cancellationToken
    ) where TEntity : IEntity;
}
