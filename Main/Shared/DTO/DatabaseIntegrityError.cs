using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public class DatabaseIntegrityError(
    string _entityType,
    IEntity _entity,
    string _errorMessage,
    params ConflictEntity[] _conflictEntities
)
{
    [JsonPropertyName(JsonPropertyNames.ENTITY_TYPE)]
    public string EntityType { get; } = _entityType;

    [JsonPropertyName(JsonPropertyNames.ENTITY)]
    public IEntity? Entity { get; } = _entity;

    [JsonPropertyName(JsonPropertyNames.ERROR_MESSAGE)]
    public string ErrorMessage { get; } = _errorMessage;

    [JsonPropertyName(JsonPropertyNames.CONFLICT_ENTITIES)]
    public IEnumerable<ConflictEntity> ConflictEntities { get; } = _conflictEntities;
}
