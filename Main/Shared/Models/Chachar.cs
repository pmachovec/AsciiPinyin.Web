using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table("chachar")]
public class Chachar : IEntity
{
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Required]
    public char TheCharacter { get; set; } = '\x0000';

    [JsonPropertyName("ascii_pinyin")]
    [Column("ascii_pinyin")]
    [Required]
    public string AsciiPinyin { get; set; } = "";

    [JsonPropertyName("ipa")]
    [Column("ipa")]
    [Required]
    public string Ipa { get; set; } = "";

    /*
     * The number of strokes can't be lower than 1 => using unsigned type is possible.
     * Highest theoretically possible value is 84 => byte is enough (byte is unsigned, signed would be sbyte).
     */
    [JsonPropertyName("strokes")]
    [Column("strokes")]
    [Required]
    public byte Strokes { get; set; }

    [JsonPropertyName("radical_character")]
    [Column("radical_character")]
    public char? RadicalCharacter { get; set; }

    [JsonPropertyName("radical_ascii_pinyin")]
    [Column("radical_ascii_pinyin")]
    public string? RadicalAsciiPinyin { get; set; }

    [JsonPropertyName("radical_alternative_character")]
    [Column("radical_alternative_character")]
    public char? RadicalAlternativeCharacter { get; set; }
    public Chachar? RadicalChachar { get; set; }

    public Alternative? RadicalAlternative { get; set; }

    public override bool Equals(object? other)
    {
        return other is Chachar otherChachar && otherChachar.TheCharacter == TheCharacter && otherChachar.AsciiPinyin == AsciiPinyin;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TheCharacter, AsciiPinyin);
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
