using AsciiPinyin.Web.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.HttpClients;

public sealed partial class EntityClient(
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
                LogApiNullError(_logger, entitiesApiName);
                return new HashSet<T>();
            }

            if (!result.Any())
            {
                LogApiEmptyError(_logger, entitiesApiName);
            }

            return result;
        }
        catch (Exception e)
        {
            LogApiServerSideError(_logger, entitiesApiName);
            LogExceptionError(_logger, e);
        }

        return new HashSet<T>();
    }

    public async Task<HttpStatusCode> PostEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity
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
    private static partial void LogExceptionError(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "CREATE {entityClassName}: {entity}")]
    private static partial void LogCreateInfo(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Information, "Success")]
    private static partial void LogSuccessInfo(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Failure; status code: {statusCode}, message: {message}")]
    private static partial void LogFailureError(ILogger logger, HttpStatusCode statusCode, string message);
}
