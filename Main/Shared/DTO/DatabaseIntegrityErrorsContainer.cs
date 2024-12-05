using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO.Tools;
using AsciiPinyin.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public class DatabaseIntegrityErrorsContainer(params DatabaseIntegrityError[] _databaseIntegrityErrors) : IErrorsContainer
{
    public DatabaseIntegrityErrorsContainer(
        string entityType,
        IEntity entity,
        string errorMessage,
        params ConflictEntity[] conflictEntities
    ) : this(new DatabaseIntegrityError(entityType, entity, errorMessage, conflictEntities))
    {
    }

    [JsonPropertyName(JsonPropertyNames.ERRORS)]
    public IEnumerable<DatabaseIntegrityError> Errors { get; } = _databaseIntegrityErrors;

    public override string ToString() => JsonCreator.ToJson(this);
}
