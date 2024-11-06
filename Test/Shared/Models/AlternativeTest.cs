using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
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

    private static readonly Alternative _newAlternative = new();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{ColumnNames.THE_CHARACTER}"":""⻗"")"
            + $@"(?=.*""{ColumnNames.ORIGINAL_CHARACTER}"":""雨"")"
            + $@"(?=.*""{ColumnNames.ORIGINAL_PINYIN}"":""yu"")"
            + $@"(?=.*""{ColumnNames.ORIGINAL_TONE}"":3)"
            + $@"(?=.*""{ColumnNames.STROKES}"":8)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    private static partial Regex AlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{ColumnNames.THE_CHARACTER}"":null)"
            + $@"(?=.*""{ColumnNames.ORIGINAL_CHARACTER}"":null)"
            + $@"(?=.*""{ColumnNames.ORIGINAL_PINYIN}"":null)"
            + $@"(?=.*""{ColumnNames.ORIGINAL_TONE}"":null)"
            + $@"(?=.*""{ColumnNames.STROKES}"":null)"
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
        var alternative = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalTone = _alternative.OriginalTone
        };

        Assert.That(alternative.OriginalRealPinyin, Is.Null);
    }

    [Test]
    public void OriginalRealPinyinToneNullTest()
    {
        var alternative = new Alternative
        {
            TheCharacter = _alternative.TheCharacter,
            OriginalCharacter = _alternative.OriginalCharacter,
            OriginalPinyin = _alternative.OriginalPinyin
        };

        Assert.That(alternative.OriginalRealPinyin, Is.Null);
    }

    [Test]
    public void OriginalRealPinyinNewAlternativeTest() =>
        Assert.That(_newAlternative.OriginalRealPinyin, Is.Null);

    [Test]
    public void AlternativeToStringTest() =>
        Assert.That(_alternative.ToString(), Does.Match(AlternativeStringRegex()));

    [Test]
    public void NewAlternativeToStringTest() =>
        Assert.That(_newAlternative.ToString(), Does.Match(AllNullAlternativeStringRegex()));

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
