using AsciiPinyin.Web.Shared.Commons;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.DTO.Tools;

internal static class JsonCreator
{
    public static string ToJson(DatabaseIntegrityErrorsContainer databaseIntegrityErrorsContainer) =>
        JsonSerializer.Serialize(databaseIntegrityErrorsContainer, JsonCreatorCommons.Options);

    public static string ToJson(EntityFieldsErrorsContainer fieldsErrorsContainer) =>
        JsonSerializer.Serialize(fieldsErrorsContainer, JsonCreatorCommons.Options);
}
