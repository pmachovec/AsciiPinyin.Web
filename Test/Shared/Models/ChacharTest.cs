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

    [Test]
    public void RadicalChacharToStringTest() =>
        Assert.That(_radicalChachar.ToString(), Does.Match(RadicalChacharStringRegex()));

    [Test]
    public void NonRadicalChacharWithAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithAlternative.ToString(), Does.Match(NonRadicalChacharWithAlternativeStringRegex()));

    [Test]
    public void NonRadicalChacharWithoutAlternativeToStringTest() =>
        Assert.That(_nonRadicalChacharWithoutAlternative.ToString(), Does.Match(NonRadicalChacharWithoutAlternativeStringRegex()));
}
