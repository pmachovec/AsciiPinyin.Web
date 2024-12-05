using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO.Tools;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public class FieldErrorsContainer(params FieldError[] _fieldErrors) : IErrorsContainer
{
    [JsonPropertyName(JsonPropertyNames.ERRORS)]
    public IEnumerable<FieldError> Errors { get; } = _fieldErrors;

    public override string ToString() => JsonCreator.ToJson(this);
}
