using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table(TableNames.CHACHAR)]
public sealed class Chachar : IEntity
{
    /*
     * The string type must be used even for single characters.
     * The char type tends to malfunction when sent over HTTP requests.
     */
    [JsonPropertyName(ColumnNames.THE_CHARACTER)]
    [Column(ColumnNames.THE_CHARACTER)]
    public string? TheCharacter { get; set; }

    [JsonPropertyName(ColumnNames.PINYIN)]
    [Column(ColumnNames.PINYIN)]
    public string? Pinyin { get; set; }

    [JsonPropertyName(ColumnNames.TONE)]
    [Column(ColumnNames.TONE)]
    public byte? Tone { get; set; }

    [JsonPropertyName(ColumnNames.IPA)]
    [Column(ColumnNames.IPA)]
    public string? Ipa { get; set; }

    /*
     * The number of strokes can't be lower than 1 => using unsigned type is possible.
     * Highest theoretically possible value is 84 => byte is enough (byte is unsigned, signed would be sbyte).
     */
    [JsonPropertyName(ColumnNames.STROKES)]
    [Column(ColumnNames.STROKES)]
    public byte? Strokes { get; set; }

    [JsonPropertyName(ColumnNames.RADICAL_CHARACTER)]
    [Column(ColumnNames.RADICAL_CHARACTER)]
    public string? RadicalCharacter { get; set; }

    [JsonPropertyName(ColumnNames.RADICAL_PINYIN)]
    [Column(ColumnNames.RADICAL_PINYIN)]
    public string? RadicalPinyin { get; set; }

    [JsonPropertyName(ColumnNames.RADICAL_TONE)]
    [Column(ColumnNames.RADICAL_TONE)]
    public byte? RadicalTone { get; set; }

    [JsonPropertyName(ColumnNames.RADICAL_ALTERNATIVE_CHARACTER)]
    [Column(ColumnNames.RADICAL_ALTERNATIVE_CHARACTER)]
    public string? RadicalAlternativeCharacter { get; set; }

    // To be replaced by conversion to showing tones by diacritics.
    [JsonIgnore]
    public string? RealPinyin =>
        Pinyin is null || Tone is null ? null : Pinyin + Tone;

    [JsonIgnore]
    public bool IsRadical => RadicalCharacter is null;

    public static bool operator ==(Chachar left, Chachar right) => Comparator.EqualsForOperator(left, right);

    public static bool operator !=(Chachar left, Chachar right) => !(left == right);

    public override bool Equals(object? obj)
    {
        return obj is Chachar otherChachar
            && otherChachar.TheCharacter == TheCharacter
            && otherChachar.Pinyin == Pinyin
            && otherChachar.Tone == Tone;
    }

    public override int GetHashCode() => HashCode.Combine(TheCharacter, Pinyin, Tone);

    public override string ToString() => JsonCreator.ToJson(this);
}
