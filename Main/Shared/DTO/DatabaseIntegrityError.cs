using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class DatabaseIntegrityError
{
    public DatabaseIntegrityError(
        Chachar chachar,
        string errorMessage,
        IEnumerable<ConflictEntity> conflictEntities
    )
    {
        EntityType = TableNames.CHACHAR;
        Entity = chachar;
        ErrorMessage = errorMessage;
        ConflictEntities = conflictEntities;
    }

    public DatabaseIntegrityError(
        Alternative alternative,
        string errorMessage,
        IEnumerable<ConflictEntity> conflictEntities
    )
    {
        EntityType = TableNames.ALTERNATIVE;
        Entity = alternative;
        ErrorMessage = errorMessage;
        ConflictEntities = conflictEntities;
    }

    [JsonPropertyName(JsonPropertyNames.ENTITY_TYPE)]
    public string EntityType { get; }

    [JsonPropertyName(JsonPropertyNames.ENTITY)]
    public IEntity? Entity { get; }

    [JsonPropertyName(JsonPropertyNames.ERROR_MESSAGE)]
    public string ErrorMessage { get; }

    [JsonPropertyName(JsonPropertyNames.CONFLICT_ENTITIES)]
    public IEnumerable<ConflictEntity> ConflictEntities { get; }
}
