using AsciiPinyin.Web.Shared.Models;
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
            + @"(?=.*""the_character"":""雨"")"
            + @"(?=.*""pinyin"":""yu"")"
            + @"(?=.*""ipa"":""y:"")"
            + @"(?=.*""tone"":3)"
            + @"(?=.*""strokes"":8)"
            + @"(?=.*""radical_character"":null)"
            + @"(?=.*""radical_pinyin"":null)"
            + @"(?=.*""radical_tone"":null)"
            + @"(?=.*""radical_alternative_character"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex RadicalChacharStringRegex();

    [GeneratedRegex(
        @"^\{"
            + @"(?=.*""the_character"":""零"")"
            + @"(?=.*""pinyin"":""ling"")"
            + @"(?=.*""ipa"":""liŋ"")"
            + @"(?=.*""tone"":2)"
            + @"(?=.*""strokes"":13)"
            + @"(?=.*""radical_character"":""雨"")"
            + @"(?=.*""radical_pinyin"":""yu"")"
            + @"(?=.*""radical_tone"":3)"
            + @"(?=.*""radical_alternative_character"":""⻗"")"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex NonRadicalChacharWithAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + @"(?=.*""the_character"":""四"")"
            + @"(?=.*""pinyin"":""si"")"
            + @"(?=.*""ipa"":""sɹ̩"")"
            + @"(?=.*""tone"":4)"
            + @"(?=.*""strokes"":5)"
            + @"(?=.*""radical_character"":""儿"")"
            + @"(?=.*""radical_pinyin"":""er"")"
            + @"(?=.*""radical_tone"":2)"
            + @"(?=.*""radical_alternative_character"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex NonRadicalChacharWithoutAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + @"(?=.*""the_character"":null)"
            + @"(?=.*""pinyin"":null)"
            + @"(?=.*""ipa"":null)"
            + @"(?=.*""tone"":null)"
            + @"(?=.*""strokes"":null)"
            + @"(?=.*""radical_character"":null)"
            + @"(?=.*""radical_pinyin"":null)"
            + @"(?=.*""radical_tone"":null)"
            + @"(?=.*""radical_alternative_character"":null)"
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
