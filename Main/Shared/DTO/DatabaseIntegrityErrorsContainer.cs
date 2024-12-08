using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO.Tools;
using AsciiPinyin.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class DatabaseIntegrityErrorsContainer(params DatabaseIntegrityError[] _databaseIntegrityErrors) : IErrorsContainer
{
    public DatabaseIntegrityErrorsContainer(
        Chachar chachar,
        string errorMessage,
        params ConflictEntity[] conflictEntities
    ) : this(new DatabaseIntegrityError(chachar, errorMessage, conflictEntities))
    {
    }

    public DatabaseIntegrityErrorsContainer(
        Alternative alternative,
        string errorMessage,
        params ConflictEntity[] conflictEntities
    ) : this(new DatabaseIntegrityError(alternative, errorMessage, conflictEntities))
    {
    }

    [JsonPropertyName(JsonPropertyNames.DATABASE_INTEGRITY_ERRORS)]
    public IEnumerable<DatabaseIntegrityError> Errors { get; } = _databaseIntegrityErrors;

    public override string ToString() => JsonCreator.ToJson(this);
}
