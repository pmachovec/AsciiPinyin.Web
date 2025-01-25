using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.HttpClients;

public sealed class EntityClient(
    HttpClient _httpClient,
    ILogger<EntityClient> _logger
) : IEntityClient
{
    public async Task<ISet<T>> GetEntitiesAsync<T>(
        string entitiesApiName,
        CancellationToken cancellationToken
    ) where T : IEntity
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ISet<T>>(entitiesApiName, cancellationToken);

            if (result is null)
            {
                LogCommons.LogApiNullError(_logger, entitiesApiName);
                return new HashSet<T>();
            }

            if (!result.Any())
            {
                LogCommons.LogApiEmptyError(_logger, entitiesApiName);
            }

            return result;
        }
        catch (Exception e)
        {
            LogCommons.LogApiServerSideError(_logger, entitiesApiName);
            LogCommons.LogExceptionError(_logger, e);
        }

        return new HashSet<T>();
    }

    public async Task<HttpStatusCode> PostEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity
    {
        LogCommons.LogCreateInfo(_logger, entity.GetType(), entity);
        var result = await _httpClient.PostAsJsonAsync(entitiesApiName, entity, cancellationToken);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            LogCommons.LogSuccessInfo(_logger);
        }
        else
        {
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            LogCommons.LogFailureError(_logger, result.StatusCode, content);
        }

        return result.StatusCode;
    }
}
