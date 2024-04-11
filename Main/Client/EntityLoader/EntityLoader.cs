using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.EntityLoader;

public sealed class EntityLoader(
    HttpClient httpClient,
    IJSInteropConsole jsInteropConsole) : IEntityLoader
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
                jsInteropConsole.ConsoleError($"EntityLoader.LoadEntitiesAsync: Result of retrieving '{entitiesApiName}' is null.");
                return [];
            }

            if (!result.Any())
            {
                jsInteropConsole.ConsoleError($"EntityLoader.LoadEntitiesAsync: Result of retrieving '{entitiesApiName}' is empty.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _jsInteropConsole.ConsoleError($"EntityLoader.LoadEntitiesAsync: Error occured on the server side when retrieving '{entitiesApiName}'.");
            _jsInteropConsole.ConsoleError(ex);
        }

        return [];
    }
}
