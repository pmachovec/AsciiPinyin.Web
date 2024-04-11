using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table("alternative")]
public sealed class Alternative : IEntity
{
    /*
     * The string type must be used even for single characters.
     * The char type tends to malfunction when sent over HTTP requests.
     */
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Required]
    public string TheCharacter { get; set; } = string.Empty;

    [JsonPropertyName("original_character")]
    [Column("original_character")]
    [Required]
    public string OriginalCharacter { get; set; } = string.Empty;

    [JsonPropertyName("original_pinyin")]
    [Column("original_pinyin")]
    [Required]
    public string OriginalPinyin { get; set; } = string.Empty;

    [JsonPropertyName("original_tone")]
    [Column("original_tone")]
    [Required]
    public byte OriginalTone { get; set; }

    [JsonPropertyName("strokes")]
    [Column("strokes")]
    [Required]
    public byte Strokes { get; set; }

    // To be replaced by conversion to showing tones by diacritics.
    [JsonIgnore]
    public string OriginalRealPinyin => OriginalPinyin + OriginalTone;

    public static bool operator ==(Alternative left, Alternative right) => Comparator.EqualsForOperator(left, right);

    public static bool operator !=(Alternative left, Alternative right) => !(left == right);

    public override bool Equals(object? obj)
    {
        return obj is Alternative otherAlternative
            && otherAlternative.TheCharacter == TheCharacter
            && otherAlternative.OriginalCharacter == OriginalCharacter
            && otherAlternative.OriginalPinyin == OriginalPinyin
            && otherAlternative.OriginalTone == OriginalTone;
    }

    public override int GetHashCode() => HashCode.Combine(TheCharacter, OriginalCharacter, OriginalPinyin);

    public override string ToString() => JsonCreator.ToJson(this);
}
