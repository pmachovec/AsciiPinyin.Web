using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using System.Net.Http.Json;

namespace AsciiPinyin.Web.Client.Shared.EntityLoader;

public class EntityLoader : IEntityLoader
{
    private readonly HttpClient _httpClient;
    private readonly IJSInteropConsole _jsInteropConsole;

    public EntityLoader(
        HttpClient httpClient,
        IJSInteropConsole jsInteropConsole)
    {
        _httpClient = httpClient;
        _jsInteropConsole = jsInteropConsole;
    }

    public async Task<T[]?> LoadEntitiesAsync<T>(string entitiesApiName) where T : IEntity
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T[]>(entitiesApiName);
        }
        catch
        {
            _jsInteropConsole.ConsoleWarning($"EntityLoader.LoadEntitiesAsync: Loading of {entitiesApiName} failed");
        }

        return null;
    }
}
