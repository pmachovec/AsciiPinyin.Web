using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public class ConflictEntity(
    string _entityType,
    IEntity _entity
)
{
    [JsonPropertyName(JsonPropertyNames.ENTITY_TYPE)]
    public string EntityType { get; } = _entityType;

    [JsonPropertyName(JsonPropertyNames.ENTITY)]
    public IEntity? Entity { get; } = _entity;
}
