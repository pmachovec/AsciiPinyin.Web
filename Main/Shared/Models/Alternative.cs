using AsciiPinyin.Web.Shared.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table("alternative")]
public sealed class Alternative : IEntity
{
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Required]
    public char TheCharacter { get; set; } = '\x0000';

    [JsonPropertyName("original_character")]
    [Column("original_character")]
    [Required]
    public char OriginalCharacter { get; set; } = '\x0000';

    [JsonPropertyName("original_ascii_pinyin")]
    [Column("original_ascii_pinyin")]
    [Required]
    public string OriginalAsciiPinyin { get; set; } = "";

    [JsonPropertyName("strokes")]
    [Column("strokes")]
    [Required]
    public byte Strokes { get; set; }

    public static bool operator ==(Alternative left, Alternative right)
    {
        return Comparator.EqualsForOperator(left, right);
    }

    public static bool operator !=(Alternative left, Alternative right) => !(left == right);

    public override bool Equals(object? other)
    {
        return other is Alternative otherAlternative
            && otherAlternative.TheCharacter == TheCharacter
            && otherAlternative.OriginalCharacter == OriginalCharacter
            && otherAlternative.OriginalAsciiPinyin == OriginalAsciiPinyin;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TheCharacter, OriginalCharacter, OriginalAsciiPinyin);
    }

    public override string ToString()
    {
        return JsonCreator.ToJson(this);
    }
}
