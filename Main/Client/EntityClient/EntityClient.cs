using AsciiPinyin.Web.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.EntityClient;

public sealed partial class EntityClient(
    HttpClient _httpClient,
    ILogger<EntityClient> _logger
) : IEntityClient
{
    public async Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
        string entitiesApiName,
        CancellationToken cancellationToken
    ) where TEntity : IEntity
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<TEntity>>(entitiesApiName, cancellationToken);

            if (result is null)
            {
                LogApiNull(_logger, entitiesApiName);
                return [];
            }

            if (!result.Any())
            {
                LogApiEmpty(_logger, entitiesApiName);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogApiServerSideError(_logger, entitiesApiName);
            LogException(_logger, ex);
        }

        return [];
    }

    public async Task<HttpStatusCode> PostEntityAsync<TEntity>(
        string entitiesApiName,
        TEntity entity,
        CancellationToken cancellationToken
    ) where TEntity : IEntity
    {
        LogCreate(_logger, entity.GetType(), entity);
        var result = await _httpClient.PostAsJsonAsync(entitiesApiName, entity, cancellationToken);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            LogSuccess(_logger);
        }
        else
        {
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            LogFailure(_logger, result.StatusCode, content);
        }

        return result.StatusCode;
    }

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is null.")]
    private static partial void LogApiNull(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is empty.")]
    private static partial void LogApiEmpty(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Error occured on the server side when retrieving '{entitiesApiName}'.")]
    private static partial void LogApiServerSideError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error)]
    private static partial void LogException(ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Information, "CREATE {entityClassName}: {entity}")]
    private static partial void LogCreate(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Information, "Success")]
    private static partial void LogSuccess(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Failure; status code: {statusCode}, message: {message}")]
    private static partial void LogFailure(ILogger logger, HttpStatusCode statusCode, string message);
}
