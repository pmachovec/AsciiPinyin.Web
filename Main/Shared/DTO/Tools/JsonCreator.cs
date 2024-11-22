using AsciiPinyin.Web.Shared.Commons;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.DTO.Tools;

internal static class JsonCreator
{
    public static string ToJson(FieldErrorsContainer fieldErrorsContainer) =>
        JsonSerializer.Serialize(fieldErrorsContainer, JsonCreatorCommons.Options);
}
