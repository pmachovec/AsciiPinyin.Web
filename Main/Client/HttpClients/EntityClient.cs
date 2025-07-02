using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.HttpClients;

// Methods used on HttpClient come from the extension HttpClientJsonExtensions.
// And as methods from extensions can't be mocked, this thing can't be tested.
// This is basically just a wrapper around HttpClient. To test it, you would have to write another untestable wrapper.
[ExcludeFromCodeCoverage]
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
                LogCommons.LogApiNullError(_logger, HttpMethod.Get, entitiesApiName);
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
    ) where T : IEntity =>
        await SubmitEntityCommonsAsync(
            LogCommons.LogCreateInfo,
            entitiesApiName,
            entity,
            _httpClient.PostAsJsonAsync,
            HttpMethod.Post,
            cancellationToken
        );

    public async Task<HttpStatusCode> PostDeleteEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity =>
        await SubmitEntityCommonsAsync(
            LogCommons.LogDeleteInfo,
            $"{entitiesApiName}/{ApiNames.DELETE}",
            entity,
            _httpClient.PostAsJsonAsync,
            HttpMethod.Post,
            cancellationToken
        );

    public async Task<HttpStatusCode> PutEntityAsync<T>(
        string entitiesApiName,
        T entity,
        CancellationToken cancellationToken
    ) where T : IEntity =>
        await SubmitEntityCommonsAsync(
            LogCommons.LogDeleteInfo,
            entitiesApiName,
            entity,
            _httpClient.PutAsJsonAsync,
            HttpMethod.Put,
            cancellationToken
        );

    private async Task<HttpStatusCode> SubmitEntityCommonsAsync<T>(
        Action<ILogger<EntityClient>, Type, IEntity> logAction,
        string entitiesApiName,
        T entity,
        Func<string, T, CancellationToken, Task<HttpResponseMessage>> submitAsJsonAsync,
        HttpMethod httpMethod,
        CancellationToken cancellationToken
    ) where T : IEntity
    {
        logAction(_logger, entity.GetType(), entity);
        HttpResponseMessage? result;

        try
        {
            result = await submitAsJsonAsync(entitiesApiName, entity, cancellationToken);
        }
        catch (Exception e)
        {
            LogCommons.LogExceptionError(_logger, e);
            return HttpStatusCode.InternalServerError;
        }

        if (result is null)
        {
            LogCommons.LogApiNullError(_logger, httpMethod, entitiesApiName);
            return HttpStatusCode.InternalServerError;
        }

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
