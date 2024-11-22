using AsciiPinyin.Web.Shared.DTO.Tools;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public class FieldErrorsContainer
{
    [JsonPropertyName("errors")]
    public IDictionary<string, FieldError> Errors { get; } = new Dictionary<string, FieldError>();

    public FieldErrorsContainer(IEnumerable<FieldError> fieldErrors) =>
        Errors = fieldErrors.ToDictionary(f => f.FieldJsonPropertyName);

    public FieldErrorsContainer(params FieldError[] fieldErrors) =>
        Errors = fieldErrors.ToDictionary(f => f.FieldJsonPropertyName);

    public override string ToString() => JsonCreator.ToJson(this);
}
