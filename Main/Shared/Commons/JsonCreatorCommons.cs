using System.Text.Encodings.Web;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.Commons;

internal static class JsonCreatorCommons
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
