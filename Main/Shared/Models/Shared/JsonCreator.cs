using System.Text.Encodings.Web;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.Models.Shared;

internal static class JsonCreator
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToJson(IEntity entity)
    {
        return JsonSerializer.Serialize(entity, _options);
    }
}
