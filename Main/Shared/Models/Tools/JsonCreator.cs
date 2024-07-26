using System.Text.Encodings.Web;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.Models.Tools;

internal static class JsonCreator
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToJson(IEntity entity) => JsonSerializer.Serialize(entity, entity.GetType(), _options);
}
