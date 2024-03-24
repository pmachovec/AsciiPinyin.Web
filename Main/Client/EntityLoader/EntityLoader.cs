using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.EntityLoader;

public class EntityLoader(
    HttpClient httpClient,
    IJSInteropConsole jsInteropConsole) : IEntityLoader
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IJSInteropConsole _jsInteropConsole = jsInteropConsole;

    public async Task<T[]?> LoadEntitiesAsync<T>(string entitiesApiName) where T : IEntity
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T[]>(entitiesApiName);
        }
        catch
        {
            _jsInteropConsole.ConsoleWarning($"EntityLoader.LoadEntitiesAsync: Error occured on the server side when retrieving '{entitiesApiName}'");
        }

        return null;
    }
}
