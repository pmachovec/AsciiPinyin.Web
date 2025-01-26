using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.HttpClients;

public interface IEntityClient
{
    Task<ISet<T>> GetEntitiesAsync<T>(
        string entitiesApiName,
        CancellationToken cancellationToken
    ) where T : IEntity;

    Task<HttpStatusCode> PostEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity;

    Task<HttpStatusCode> PostDeleteEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity;
}
