using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[JsonDerivedType(typeof(Alternative))]
[JsonDerivedType(typeof(Chachar))]
public interface IEntity
{
    // The string type must be used even for single characters.
    // The char type tends to malfunction when sent over HTTP requests.
    string? TheCharacter { get; set; }

    // Must use signed number types, because blazor client-side model validation doesn't work with unsigned numeric types.
    // The 'sbyte' would be enough in all situations, but the blazor client-side model validation doesn't work with it => must use 'short'.
    short? Strokes { get; set; }

    bool AreAllFieldsEqual(object? obj);
}
