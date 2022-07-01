using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Shared.EntityLoader;

public interface IEntityLoader
{
    Task<T[]?> LoadEntitiesAsync<T>(string entitiesApiName) where T : IEntity;
}
