using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table("chachar")]
public sealed class Chachar : IEntity
{
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Required]
    public char TheCharacter { get; set; }

    [JsonPropertyName("ascii_pinyin")]
    [Column("ascii_pinyin")]
    [Required]
    public string AsciiPinyin { get; set; } = string.Empty;

    [JsonPropertyName("tone")]
    [Column("tone")]
    [Required]
    public byte Tone { get; set; }

    [JsonPropertyName("ipa")]
    [Column("ipa")]
    [Required]
    public string Ipa { get; set; } = string.Empty;

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

    [JsonPropertyName("radical_tone")]
    [Column("radical_tone")]
    public byte? RadicalTone { get; set; }

    [JsonPropertyName("radical_alternative_character")]
    [Column("radical_alternative_character")]
    public char? RadicalAlternativeCharacter { get; set; }

    public Chachar? RadicalChachar { get; set; }

    public Alternative? RadicalAlternative { get; set; }

    public static bool operator ==(Chachar left, Chachar right) => Comparator.EqualsForOperator(left, right);

    public static bool operator !=(Chachar left, Chachar right) => !(left == right);

    public override bool Equals(object? obj)
    {
        return obj is Chachar otherChachar
            && otherChachar.TheCharacter == TheCharacter
            && otherChachar.AsciiPinyin == AsciiPinyin
            && otherChachar.Tone == Tone;
    }

    public override int GetHashCode() => HashCode.Combine(TheCharacter, AsciiPinyin, Tone);

    public override string ToString() => JsonCreator.ToJson(this);
}
