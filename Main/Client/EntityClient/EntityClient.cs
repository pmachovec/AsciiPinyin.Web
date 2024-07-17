using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.EntityClient;

public sealed class EntityClient(
    HttpClient httpClient,
    IJSInteropConsole jsInteropConsole
) : IEntityClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IJSInteropConsole _jsInteropConsole = jsInteropConsole;

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
                _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(GetEntitiesAsync)}: Result of retrieving '{entitiesApiName}' is null.");
                return [];
            }

            if (!result.Any())
            {
                _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(GetEntitiesAsync)}: Result of retrieving '{entitiesApiName}' is empty.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(GetEntitiesAsync)}: Error occured on the server side when retrieving '{entitiesApiName}'.");
            _jsInteropConsole.ConsoleError(ex);
        }

        return [];
    }

    public async Task<HttpStatusCode> PostEntityAsync<TEntity>(
        string entitiesApiName,
        TEntity entity,
        CancellationToken cancellationToken
    ) where TEntity : IEntity
    {
        _jsInteropConsole.ConsoleInfo($"CREATE {entity.GetType()}: {entity}");
        var result = await _httpClient.PostAsJsonAsync(entitiesApiName, entity, cancellationToken);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            _jsInteropConsole.ConsoleInfo("Success");
        }
        else
        {
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            _jsInteropConsole.ConsoleError("Failure");
            _jsInteropConsole.ConsoleError($"Status code: {result.StatusCode}, message: {content}");
        }

        return result.StatusCode;
    }
}
