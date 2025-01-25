using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Commons;
using NUnit.Framework;

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
        Assert.That(_radicalChachar.ToString(), Does.Match(Regexes.RadicalChacharStringRegex()));

    [Test]
    public void NonRadicalChacharWithAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithAlternative.ToString(), Does.Match(Regexes.NonRadicalChacharWithAlternativeStringRegex()));

    [Test]
    public void NonRadicalChacharWithoutAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithoutAlternative.ToString(), Does.Match(Regexes.NonRadicalChacharWithoutAlternativeStringRegex()));

    [Test]
    public void NewChacharToStringTest() =>
        Assert.That(_newChachar.ToString(), Does.Match(Regexes.AllNullChacharStringRegex()));

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
