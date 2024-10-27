using AsciiPinyin.Web.Shared.Models.Tools;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

public class FieldErrorsContainer
{
    [JsonPropertyName("errors")]
    public IDictionary<string, FieldError> Errors { get; } = new Dictionary<string, FieldError>();

    public FieldErrorsContainer(IEnumerable<FieldError> fieldErrors)
    {
        foreach (var fieldError in fieldErrors)
        {
            Errors[fieldError.FieldJsonPropertyName] = fieldError;
        }
    }

    public override string ToString() => JsonCreator.ToJson(this);
}
