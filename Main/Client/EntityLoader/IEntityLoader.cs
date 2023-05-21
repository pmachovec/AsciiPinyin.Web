using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.EntityLoader;

public interface IEntityLoader
{
    Task<T[]?> LoadEntitiesAsync<T>(string entitiesApiName) where T : IEntity;
}
