using AsciiPinyin.Web.Shared.Commons;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.DTO.Tools;

internal static class JsonCreator
{
    public static string ToJson(DatabaseIntegrityErrorsContainer entityErrorsContainer) =>
        JsonSerializer.Serialize(entityErrorsContainer, JsonCreatorCommons.Options);

    public static string ToJson(FieldsErrorsContainer fieldsErrorsContainer) =>
        JsonSerializer.Serialize(fieldsErrorsContainer, JsonCreatorCommons.Options);
}
