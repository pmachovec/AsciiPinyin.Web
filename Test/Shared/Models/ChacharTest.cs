using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Test.Models;

[TestFixture]
internal sealed partial class ChacharTest
{
    private static readonly Chachar _radicalChachar = new()
    {
        TheCharacter = "雨",
        Pinyin = "yu",
        Ipa = "y:",
        Tone = 3,
        Strokes = 8
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative = new()
    {
        TheCharacter = "零",
        Pinyin = "ling",
        Ipa = "liŋ",
        Tone = 2,
        Strokes = 13,
        RadicalCharacter = "雨",
        RadicalPinyin = "yu",
        RadicalTone = 3,
        RadicalAlternativeCharacter = "⻗"
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative = new()
    {
        TheCharacter = "四",
        Pinyin = "si",
        Ipa = "sɹ̩",
        Tone = 4,
        Strokes = 5,
        RadicalCharacter = "儿",
        RadicalPinyin = "er",
        RadicalTone = 2
    };

    private static readonly Chachar _newChachar = new();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""雨"")"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""yu"")"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""y:"")"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":3)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":8)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex RadicalChacharStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""零"")"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""ling"")"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""liŋ"")"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":2)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":13)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":""雨"")"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":""yu"")"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":3)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":""⻗"")"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex NonRadicalChacharWithAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""四"")"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""si"")"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""sɹ̩"")"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":4)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":5)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":""儿"")"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":""er"")"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":2)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex NonRadicalChacharWithoutAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":null)"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":null)"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":null)"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":null)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":null)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex AllNullChacharStringRegex();

    [Test]
    public void RealPinyinTest() =>
        Assert.That(_radicalChachar.RealPinyin, Is.EqualTo("yu3"));

    [Test]
    public void RealPinyinPinyinNullTest()
    {
        var chachar = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Tone = _nonRadicalChacharWithAlternative.Tone
        };

        Assert.That(chachar.RealPinyin, Is.Null);
    }

    [Test]
    public void RealPinyinToneNullTest()
    {
        var chachar = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin
        };

        Assert.That(chachar.RealPinyin, Is.Null);
    }

    [Test]
    public void RealPinyinNewChacharTest() =>
        Assert.That(_newChachar.RealPinyin, Is.Null);

    [Test]
    public void RadicalChacharToStringTest() =>
        Assert.That(_radicalChachar.ToString(), Does.Match(RadicalChacharStringRegex()));

    [Test]
    public void NonRadicalChacharWithAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithAlternative.ToString(), Does.Match(NonRadicalChacharWithAlternativeStringRegex()));

    [Test]
    public void NonRadicalChacharWithoutAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithoutAlternative.ToString(), Does.Match(NonRadicalChacharWithoutAlternativeStringRegex()));

    [Test]
    public void NewChacharToStringTest() =>
        Assert.That(_newChachar.ToString(), Does.Match(AllNullChacharStringRegex()));

    [Test]
    public void ChacharEqualsFullCloneTest()
    {
        var chacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(chacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(chacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void ChacharEqualsMinimalCloneTest()
    {
        var chacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone
        };

        Assert.That(chacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(chacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void ChacharEqualsNonKeyFieldDiffersTest()
    {
        var chacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = (byte?)(_nonRadicalChacharWithAlternative.RadicalTone + 1),
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(chacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(chacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void ChacharEqualsKeyFieldDiffersTest()
    {
        var chacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = (byte)(_nonRadicalChacharWithAlternative.Tone! + 1),
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(chacharClone, Is.Not.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(chacharClone != _nonRadicalChacharWithAlternative, Is.True);
    }
}
