using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table(TableNames.ALTERNATIVE)]
public sealed class Alternative : IEntity
{
    [
        Column(JsonPropertyNames.THE_CHARACTER),
        DisplayName(JsonPropertyNames.THE_CHARACTER),
        JsonPropertyName(JsonPropertyNames.THE_CHARACTER)
    ]
    public string? TheCharacter { get; set; }

    [
        Column(JsonPropertyNames.ORIGINAL_CHARACTER),
        DisplayName(JsonPropertyNames.ORIGINAL_CHARACTER),
        JsonPropertyName(JsonPropertyNames.ORIGINAL_CHARACTER)
    ]
    public string? OriginalCharacter { get; set; }

    [
        Column(JsonPropertyNames.ORIGINAL_PINYIN),
        DisplayName(JsonPropertyNames.ORIGINAL_PINYIN),
        JsonPropertyName(JsonPropertyNames.ORIGINAL_PINYIN)
    ]
    public string? OriginalPinyin { get; set; }

    [
        Column(JsonPropertyNames.ORIGINAL_TONE),
        DisplayName(JsonPropertyNames.ORIGINAL_TONE),
        JsonPropertyName(JsonPropertyNames.ORIGINAL_TONE)
    ]
    public short? OriginalTone { get; set; }

    [
        Column(JsonPropertyNames.STROKES),
        DisplayName(JsonPropertyNames.STROKES),
        JsonPropertyName(JsonPropertyNames.STROKES)
    ]
    public short? Strokes { get; set; }

    // To be replaced by conversion to showing tones by diacritics.
    [JsonIgnore]
    public string? OriginalRealPinyin =>
        OriginalPinyin is null || OriginalTone is null ? null : OriginalPinyin + OriginalTone;

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
