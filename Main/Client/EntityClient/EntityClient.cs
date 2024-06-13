using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.EntityClient;

public sealed class EntityClient(
    HttpClient httpClient,
    IJSInteropConsole jsInteropConsole) : IEntityClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IJSInteropConsole _jsInteropConsole = jsInteropConsole;

    public async Task<IEnumerable<TEntity>> LoadEntitiesAsync<TEntity>(
        string entitiesApiName,
        CancellationToken cancellationToken) where TEntity : IEntity
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<TEntity>>(entitiesApiName, cancellationToken);

            if (result is null)
            {
                _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(LoadEntitiesAsync)}: Result of retrieving '{entitiesApiName}' is null.");
                return [];
            }

            if (!result.Any())
            {
                _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(LoadEntitiesAsync)}: Result of retrieving '{entitiesApiName}' is empty.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _jsInteropConsole.ConsoleError($"{nameof(EntityClient)}.{nameof(LoadEntitiesAsync)}: Error occured on the server side when retrieving '{entitiesApiName}'.");
            _jsInteropConsole.ConsoleError(ex);
        }

        return [];
    }
}
