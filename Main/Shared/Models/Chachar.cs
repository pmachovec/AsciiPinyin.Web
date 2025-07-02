using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table(TableNames.CHACHAR)]
public sealed class Chachar : IEntity
{
    public Chachar() { }

    public Chachar(Chachar chachar)
    {
        TheCharacter = chachar.TheCharacter;
        Pinyin = chachar.Pinyin;
        Tone = chachar.Tone;
        Ipa = chachar.Ipa;
        Strokes = chachar.Strokes;
        RadicalCharacter = chachar.RadicalCharacter;
        RadicalPinyin = chachar.RadicalPinyin;
        RadicalTone = chachar.RadicalTone;
        RadicalAlternativeCharacter = chachar.RadicalAlternativeCharacter;
    }

    [
        Column(JsonPropertyNames.THE_CHARACTER),
        DisplayName(JsonPropertyNames.THE_CHARACTER),
        JsonPropertyName(JsonPropertyNames.THE_CHARACTER)
    ]
    public string? TheCharacter { get; set; }

    [
        Column(JsonPropertyNames.PINYIN),
        DisplayName(JsonPropertyNames.PINYIN),
        JsonPropertyName(JsonPropertyNames.PINYIN)
    ]
    public string? Pinyin { get; set; }

    [
        Column(JsonPropertyNames.TONE),
        DisplayName(JsonPropertyNames.TONE),
        JsonPropertyName(JsonPropertyNames.TONE)
    ]
    public short? Tone { get; set; }

    [
        Column(JsonPropertyNames.IPA),
        DisplayName(JsonPropertyNames.IPA),
        JsonPropertyName(JsonPropertyNames.IPA)
    ]
    public string? Ipa { get; set; }

    [
        Column(JsonPropertyNames.STROKES),
        DisplayName(JsonPropertyNames.STROKES),
        JsonPropertyName(JsonPropertyNames.STROKES)
    ]
    public short? Strokes { get; set; }

    [
        Column(JsonPropertyNames.RADICAL_CHARACTER),
        DisplayName(JsonPropertyNames.RADICAL_CHARACTER),
        JsonPropertyName(JsonPropertyNames.RADICAL_CHARACTER)
    ]
    public string? RadicalCharacter { get; set; }

    [
        Column(JsonPropertyNames.RADICAL_PINYIN),
        DisplayName(JsonPropertyNames.RADICAL_PINYIN),
        JsonPropertyName(JsonPropertyNames.RADICAL_PINYIN)
    ]
    public string? RadicalPinyin { get; set; }

    [
        Column(JsonPropertyNames.RADICAL_TONE),
        DisplayName(JsonPropertyNames.RADICAL_TONE),
        JsonPropertyName(JsonPropertyNames.RADICAL_TONE)
    ]
    public short? RadicalTone { get; set; }

    [
        Column(JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER),
        DisplayName(JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER),
        JsonPropertyName(JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER)
    ]
    public string? RadicalAlternativeCharacter { get; set; }

    // To be replaced by conversion to showing tones by diacritics.
    [JsonIgnore]
    public string? RealPinyin =>
        Pinyin is null || Tone is null ? null : Pinyin + Tone;

    [JsonIgnore]
    public bool IsRadical => RadicalCharacter is null;

    public bool AreAllFieldsEqual(object? obj) =>
        obj is Chachar otherChachar
        && Equals(otherChachar)
        && otherChachar.Ipa == Ipa
        && otherChachar.Strokes == Strokes
        && otherChachar.RadicalCharacter == RadicalCharacter
        && otherChachar.RadicalPinyin == RadicalPinyin
        && otherChachar.RadicalTone == RadicalTone
        && otherChachar.RadicalAlternativeCharacter == RadicalAlternativeCharacter;

    public static bool operator ==(Chachar left, Chachar right) => Comparator.EqualsForOperator(left, right);

    public static bool operator !=(Chachar left, Chachar right) => !(left == right);

    public override bool Equals(object? obj) =>
        obj is Chachar otherChachar
        && otherChachar.TheCharacter == TheCharacter
        && otherChachar.Pinyin == Pinyin
        && otherChachar.Tone == Tone;

    public override int GetHashCode() => HashCode.Combine(TheCharacter, Pinyin, Tone);

    public override string ToString() => JsonCreator.ToJson(this);
}
