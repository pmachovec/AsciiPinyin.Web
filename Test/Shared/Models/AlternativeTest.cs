using AsciiPinyin.Web.Shared.Models;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Test.Models;

[TestFixture]
internal sealed partial class AlternativeTest
{
    private static readonly Alternative _alternative = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = "雨",
        OriginalPinyin = "yu",
        OriginalTone = 3,
        Strokes = 8
    };

    [GeneratedRegex(
        @"^\{"
            + @"(?=.*""the_character"":""⻗"")"
            + @"(?=.*""original_character"":""雨"")"
            + @"(?=.*""original_pinyin"":""yu"")"
            + @"(?=.*""original_tone"":3)"
            + @"(?=.*""strokes"":8)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex AlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + @"(?=.*""the_character"":null)"
            + @"(?=.*""original_character"":null)"
            + @"(?=.*""original_pinyin"":null)"
            + @"(?=.*""original_tone"":null)"
            + @"(?=.*""strokes"":null)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex AllNullAlternativeStringRegex();

    [Test]
    public void OriginalRealPinyinTest() =>
        Assert.That(_alternative.OriginalRealPinyin, Is.EqualTo("yu3"));

    [Test]
    public void OriginalRealPinyinPinyinNullTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalTone = _alternative.OriginalTone
        };

        Assert.That(alternativeClone.OriginalRealPinyin, Is.Null);
    }

    [Test]
    public void OriginalRealPinyinToneNullTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin
        };

        Assert.That(alternativeClone.OriginalRealPinyin, Is.Null);
    }

    [Test]
    public void AlternativeToStringTest() =>
        Assert.That(_alternative.ToString(), Does.Match(AlternativeStringRegex()));

    [Test]
    public void NewAlternativeToStringTest() =>
        Assert.That(new Alternative().ToString(), Does.Match(AllNullAlternativeStringRegex()));

    [Test]
    public void AlternativeEqualsFullCloneTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin,
            OriginalTone = _alternative.OriginalTone,
            Strokes = _alternative.Strokes
        };

        Assert.That(alternativeClone, Is.EqualTo(_alternative));
        Assert.That(alternativeClone == _alternative, Is.True);
    }

    [Test]
    public void AlternativeEqualsMinimalCloneTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin,
            OriginalTone = _alternative.OriginalTone
        };

        Assert.That(alternativeClone, Is.EqualTo(_alternative));
        Assert.That(alternativeClone == _alternative, Is.True);
    }

    [Test]
    public void AlternativeEqualsNonKeyFieldDiffersTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin,
            OriginalTone = _alternative.OriginalTone,
            Strokes = (byte)(_alternative.Strokes! + 1)
        };

        Assert.That(alternativeClone, Is.EqualTo(_alternative));
        Assert.That(alternativeClone == _alternative, Is.True);
    }

    [Test]
    public void AlternativeEqualsKeyFieldDiffersTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin,
            OriginalTone = (byte)(_alternative.OriginalTone! + 1),
            Strokes = _alternative.Strokes
        };

        Assert.That(alternativeClone, Is.Not.EqualTo(_alternative));
        Assert.That(alternativeClone != _alternative, Is.True);
    }
}
