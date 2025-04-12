using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Commons;
using NUnit.Framework;

namespace AsciiPinyin.Web.Shared.Test.Models;

[TestFixture]
internal sealed partial class ChacharTest
{
    private const string ADDITIONAL_ALTERNATIVE_CHARACTER = "⻍";

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
    public void RadicalChacharEqualsTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone, Is.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.True);
    }

    [Test]
    public void RadicalChacharEqualsToneDiffersTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = (short)(_radicalChachar.Tone! + 1),
            Ipa = _radicalChachar.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone, Is.Not.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.False);
    }

    [Test]
    public void RadicalChacharEqualsToneNullTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = (short)(_radicalChachar.Tone! + 1),
            Ipa = _radicalChachar.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone, Is.Not.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.False);
    }

    [Test]
    public void RadicalChacharEqualsIpaDiffersTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone, Is.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.True);
    }

    [Test]
    public void RadicalChacharEqualsIpaNullTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone, Is.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.True);
    }

    [Test]
    public void RadicalChacharEqualsStrokesDifferTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = (short)(_radicalChachar.Strokes! + 1)
        };

        Assert.That(radicalChacharClone, Is.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.True);
    }

    [Test]
    public void RadicalChacharEqualsStrokesNullTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa
        };

        Assert.That(radicalChacharClone, Is.EqualTo(_radicalChachar));
        Assert.That(radicalChacharClone == _radicalChachar, Is.True);
    }

    [Test]
    public void RadicalChacharAreAllFieldsEqualTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone.AreAllFieldsEqual(_radicalChachar), Is.True);
    }

    [Test]
    public void RadicalChacharAreAllFieldsEqualIpaDiffersTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone.AreAllFieldsEqual(_radicalChachar), Is.False);
    }

    [Test]
    public void RadicalChacharAreAllFieldsEqualIpaNullTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Strokes = _radicalChachar.Strokes
        };

        Assert.That(radicalChacharClone.AreAllFieldsEqual(_radicalChachar), Is.False);
    }

    [Test]
    public void RadicalChacharAreAllFieldsEqualStrokesDifferTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = (short)(_radicalChachar.Strokes! + 1)
        };

        Assert.That(radicalChacharClone.AreAllFieldsEqual(_radicalChachar), Is.False);
    }

    [Test]
    public void RadicalChacharAreAllFieldsEqualStrokesNullTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Tone = _radicalChachar.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = (short)(_radicalChachar.Strokes! + 1)
        };

        Assert.That(radicalChacharClone.AreAllFieldsEqual(_radicalChachar), Is.False);
    }

    [Test]
    public void NonRadicalChacharEqualsTest()
    {
        var nonRadicalChacharClone = new Chachar
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

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsToneDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = (short)(_nonRadicalChacharWithAlternative.Tone! + 1),
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.Not.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.False);
    }

    [Test]
    public void NonRadicalChacharEqualsToneNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.Not.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.False);
    }

    [Test]
    public void NonRadicalChacharEqualsIpaDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsIpaNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsStrokesDifferTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = (short)(_nonRadicalChacharWithAlternative.Strokes! + 1),
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsStrokesNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalCharacterDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithoutAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalCharacterNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalPinyinDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithoutAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalPinyinNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalToneDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = (short?)(_nonRadicalChacharWithAlternative.RadicalTone + 1),
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalToneNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalAlternativeCharacterDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = ADDITIONAL_ALTERNATIVE_CHARACTER
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharEqualsRadicalAlternativeCharacterNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone
        };

        Assert.That(nonRadicalChacharClone, Is.EqualTo(_nonRadicalChacharWithAlternative));
        Assert.That(nonRadicalChacharClone == _nonRadicalChacharWithAlternative, Is.True);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualTest()
    {
        var nonRadicalChacharClone = new Chachar
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

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.True);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualIpaDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _radicalChachar.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualIpaNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualStrokesDifferTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = (short)(_nonRadicalChacharWithAlternative.Strokes! + 1),
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualStrokesNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalChacharDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithoutAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalChacharNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalPinyinDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithoutAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalPinyinNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalToneDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = (short)(_nonRadicalChacharWithAlternative.RadicalTone! + 1),
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalToneNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalAlternativeCharacter = _nonRadicalChacharWithAlternative.RadicalAlternativeCharacter
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalAlternativeCharacterDiffersTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone,
            RadicalAlternativeCharacter = ADDITIONAL_ALTERNATIVE_CHARACTER
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }

    [Test]
    public void NonRadicalChacharAreAllFieldsEqualRadicalAlternativeCharacterNullTest()
    {
        var nonRadicalChacharClone = new Chachar
        {
            TheCharacter = _nonRadicalChacharWithAlternative.TheCharacter,
            Pinyin = _nonRadicalChacharWithAlternative.Pinyin,
            Tone = _nonRadicalChacharWithAlternative.Tone,
            Ipa = _nonRadicalChacharWithAlternative.Ipa,
            Strokes = _nonRadicalChacharWithAlternative.Strokes,
            RadicalCharacter = _nonRadicalChacharWithAlternative.RadicalCharacter,
            RadicalPinyin = _nonRadicalChacharWithAlternative.RadicalPinyin,
            RadicalTone = _nonRadicalChacharWithAlternative.RadicalTone
        };

        Assert.That(nonRadicalChacharClone.AreAllFieldsEqual(_nonRadicalChacharWithAlternative), Is.False);
    }
}
