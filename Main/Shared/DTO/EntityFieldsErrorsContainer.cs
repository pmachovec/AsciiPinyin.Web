using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO.Tools;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class EntityFieldsErrorsContainer(params EntityFieldsError[] _fieldsErrors) : IErrorsContainer
{
    public EntityFieldsErrorsContainer(
        string entityType,
        params FieldError[] fieldErrors
    ) : this(new EntityFieldsError(entityType, fieldErrors))
    {
    }

    [JsonPropertyName(JsonPropertyNames.ENTITY_FIELDS_ERRORS)]
    public IEnumerable<EntityFieldsError> Errors { get; } = _fieldsErrors;

    public override string ToString() => JsonCreator.ToJson(this);
}
