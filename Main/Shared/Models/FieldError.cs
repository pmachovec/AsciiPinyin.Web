using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

public class FieldError(
    object? _errorValue,
    string _errorMessage,
    string _fieldJsonPropertyName
)
{
    [JsonPropertyName("value")]
    public object? ErrorValue { get; } = _errorValue;

    [JsonPropertyName("message")]
    public string ErrorMessage { get; } = _errorMessage;

    [JsonIgnore]
    public string FieldJsonPropertyName { get; } = _fieldJsonPropertyName;
}
