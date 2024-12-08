using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO.Tools;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class FieldsErrorsContainer(params FieldsError[] _fieldsErrors) : IErrorsContainer
{
    public FieldsErrorsContainer(
        string entityType,
        params FieldError[] fieldErrors
    ) : this(new FieldsError(entityType, fieldErrors))
    {
    }

    [JsonPropertyName(JsonPropertyNames.ERRORS)]
    public IEnumerable<FieldsError> Errors { get; } = _fieldsErrors;

    public override string ToString() => JsonCreator.ToJson(this);
}
