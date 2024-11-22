using AsciiPinyin.Web.Shared.Commons;
using System.Text.Json;

namespace AsciiPinyin.Web.Shared.Models.Tools;

internal static class JsonCreator
{
    public static string ToJson(IEntity entity) =>
        JsonSerializer.Serialize(entity, entity.GetType(), JsonCreatorCommons.Options);
}
