using AsciiPinyin.Web.Shared.Test.Constants;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Test.Commons;

internal static partial class Regexes
{
    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""⻗"",?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_CHARACTER}"":""雨"",?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_PINYIN}"":""yu"",?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_TONE}"":3,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":8,?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex AlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_CHARACTER}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_PINYIN}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.ORIGINAL_TONE}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":null,?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex AllNullAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null,?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex AllNullChacharStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""零"",?)"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""ling"",?)"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""liŋ"",?)"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":2,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":13,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":""雨"",?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":""yu"",?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":3,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":""⻗"",?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex NonRadicalChacharWithAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""四"",?)"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""si"",?)"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""sɹ̩"",?)"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":4,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":5)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":""儿"",?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":""er"",?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":2,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null,?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex NonRadicalChacharWithoutAlternativeStringRegex();

    [GeneratedRegex(
        @"^\{"
            + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""雨"",?)"
            + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""yu"",?)"
            + $@"(?=.*""{JsonPropertyNames.IPA}"":""y:"",?)"
            + $@"(?=.*""{JsonPropertyNames.TONE}"":3,?)"
            + $@"(?=.*""{JsonPropertyNames.STROKES}"":8,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":null,?)"
            + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":null,?)"
            + @".*\}$",
        RegexOptions.Compiled
    )]
    public static partial Regex RadicalChacharStringRegex();
}
