namespace AsciiPinyin.Web.Models;

using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Chachar
{
    [JsonPropertyName("ipa")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Ipa { get; set; } = "";

    [JsonPropertyName("pinyin")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Piniyin { get; set; } = "";

    /*
     * The number of strokes can't be lower than 1 => using unsigned type is possible.
     * Highest theoretically possible value is 84 => byte is enough (byte is unsigned, signed would be sbyte).
     */
    [JsonPropertyName("strokes")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once UnusedMember.Global
    public byte Strokes { get; set; }

    [JsonPropertyName("unicode")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string Unicode { get; set; } = "";

    /*
     * Is not read from the database Json, doesn't have 'set' configured. But must have JsonProperty, because that serves even
     * for the serialization to Json.
     */
    [JsonPropertyName("the_character")]
    public char TheCharacter
    {
        get
        {
            uint unicodeAsInt = 0;

            try
            {
                unicodeAsInt = uint.Parse(Unicode, NumberStyles.HexNumber);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine($"Failed parsing of unicode '{Unicode}'.");
            }

            if (unicodeAsInt > 0)
            {
                return (char)unicodeAsInt;
            }

            return '\x0000';
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Chachar chachar && Unicode == chachar.Unicode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Unicode, Piniyin, Ipa);
    }

    public override string ToString()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(this, options);
    }
}
