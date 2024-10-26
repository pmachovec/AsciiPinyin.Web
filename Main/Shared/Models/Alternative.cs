using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table(TableNames.ALTERNATIVE)]
public sealed class Alternative : IEntity
{
    /*
     * The string type must be used even for single characters.
     * The char type tends to malfunction when sent over HTTP requests.
     */
    [JsonPropertyName(ColumnNames.THE_CHARACTER)]
    [Column(ColumnNames.THE_CHARACTER)]
    public string? TheCharacter { get; set; }

    [JsonPropertyName(ColumnNames.ORIGINAL_CHARACTER)]
    [Column(ColumnNames.ORIGINAL_CHARACTER)]
    public string? OriginalCharacter { get; set; }

    [JsonPropertyName(ColumnNames.ORIGINAL_PINYIN)]
    [Column(ColumnNames.ORIGINAL_PINYIN)]
    public string? OriginalPinyin { get; set; }

    [JsonPropertyName(ColumnNames.ORIGINAL_TONE)]
    [Column(ColumnNames.ORIGINAL_TONE)]
    public byte? OriginalTone { get; set; }

    [JsonPropertyName(ColumnNames.STROKES)]
    [Column(ColumnNames.STROKES)]
    public byte? Strokes { get; set; }

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
