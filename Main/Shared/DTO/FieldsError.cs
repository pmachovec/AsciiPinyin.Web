using AsciiPinyin.Web.Shared.Constants;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class FieldsError(
    string _entityType,
    params FieldError[] _fieldErrors
)
{
    [JsonPropertyName(JsonPropertyNames.ENTITY_TYPE)]
    public string EntityType { get; } = _entityType;

    [JsonPropertyName(JsonPropertyNames.FIELD_ERRORS)]
    public IEnumerable<FieldError> FieldErrors { get; } = _fieldErrors;
}
