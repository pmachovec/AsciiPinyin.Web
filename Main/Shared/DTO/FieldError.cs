using AsciiPinyin.Web.Shared.Constants;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class FieldError(
    string _fieldName,
    object? _fieldValue,
    string _errorMessage
)
{
    [JsonPropertyName(JsonPropertyNames.FIELD_NAME)]
    public string FieldName { get; } = _fieldName;

    [JsonPropertyName(JsonPropertyNames.FIELD_VALUE)]
    public object? FieldValue { get; } = _fieldValue;

    [JsonPropertyName(JsonPropertyNames.ERROR_MESSAGE)]
    public string ErrorMessage { get; } = _errorMessage;
}
