using AsciiPinyin.Web.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.HttpClients;

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
                LogApiNullError(_logger, entitiesApiName);
                return [];
            }

            if (!result.Any())
            {
                LogApiEmptyError(_logger, entitiesApiName);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogApiServerSideError(_logger, entitiesApiName);
            LogExceptionError(_logger, ex);
        }

        return [];
    }

    public async Task<HttpStatusCode> PostEntityAsync<TEntity>(
        string entitiesApiName,
        TEntity entity,
        CancellationToken cancellationToken
    ) where TEntity : IEntity
    {
        LogCreateInfo(_logger, entity.GetType(), entity);
        var result = await _httpClient.PostAsJsonAsync(entitiesApiName, entity, cancellationToken);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            LogSuccessInfo(_logger);
        }
        else
        {
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            LogFailureError(_logger, result.StatusCode, content);
        }

        return result.StatusCode;
    }

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is null.")]
    private static partial void LogApiNullError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is empty.")]
    private static partial void LogApiEmptyError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Error occured on the server side when retrieving '{entitiesApiName}'.")]
    private static partial void LogApiServerSideError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error)]
    private static partial void LogExceptionError(ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Information, "CREATE {entityClassName}: {entity}")]
    private static partial void LogCreateInfo(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Information, "Success")]
    private static partial void LogSuccessInfo(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Failure; status code: {statusCode}, message: {message}")]
    private static partial void LogFailureError(ILogger logger, HttpStatusCode statusCode, string message);
}
