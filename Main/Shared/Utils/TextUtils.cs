using System.Globalization;

namespace AsciiPinyin.Web.Shared.Utils;

public static class TextUtils
{
    private static readonly IEnumerable<(uint Lower, uint Upper)> _chineseUnicodeRanges =
    [
        // CJK radicals supplement - positional alternatives of some radicals
        (0x2E80, 0x2FD5),

        // Kanbun - annotation characters used in Japanese copies of classical Chinese texts
        (0x3190, 0x319F),

        // CJK extension A - rare HAN characters
        (0x3400, 0x4DBF),

        // CJK unified ideographs - common characters used in modern Chinese and Japanese, including common variants
        (0x4E00, 0x9FCC),

        // CJK compatibility ideographs - HAN characters encoded in multiple locations in other encodings
        (0xF900, 0xFAAD),

        // CJK extension B - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese
        (0x20000, 0x2A6DF),

        // CJK extension C - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese
        (0x2A700, 0x2B73F),

        // CJK extension D - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese
        (0x2B740, 0x2B81F),

        // CJK extension E - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese
        (0x2B820, 0x2CEAF),

        // CJK extension F - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese + Sandwip
        (0x2CEB0, 0x2EBEF),

        // CJK compatibility ideographs supplement
        (0x2F800, 0x2FA1F),

        // CJK extension G - rare and historic ideographs for Chinese, Japanese, Korean and Vietnamese
        (0x30000, 0x3134F)
    ];

    private static readonly IEnumerable<uint> _ipaSymbolCodes =
    [
        0x21,
        0x27,
        0x2E,
        0x3A,
        0x61,
        0x62,
        0x63,
        0x64,
        0x65,
        0x66,
        0x67,
        0x68,
        0x69,
        0x6A,
        0x6B,
        0x6C,
        0x6D,
        0x6E,
        0x6F,
        0x70,
        0x71,
        0x72,
        0x73,
        0x74,
        0x75,
        0x76,
        0x77,
        0x78,
        0x79,
        0x7A,
        0x7C,
        0xE6,
        0xE7,
        0xF0,
        0xF8,
        0x127,
        0x14B,
        0x153,
        0x1C0,
        0x1C1,
        0x1C2,
        0x250,
        0x251,
        0x252,
        0x253,
        0x254,
        0x255,
        0x256,
        0x257,
        0x258,
        0x259,
        0x25A,
        0x25B,
        0x25C,
        0x25E,
        0x25F,
        0x260,
        0x261,
        0x262,
        0x263,
        0x264,
        0x265,
        0x266,
        0x267,
        0x268,
        0x26A,
        0x26B,
        0x26C,
        0x26D,
        0x26E,
        0x26F,
        0x270,
        0x271,
        0x272,
        0x273,
        0x274,
        0x275,
        0x276,
        0x278,
        0x279,
        0x27A,
        0x27B,
        0x27D,
        0x27E,
        0x280,
        0x281,
        0x282,
        0x283,
        0x284,
        0x288,
        0x289,
        0x28A,
        0x28B,
        0x28C,
        0x28D,
        0x28E,
        0x28F,
        0x290,
        0x291,
        0x292,
        0x294,
        0x295,
        0x298,
        0x299,
        0x29B,
        0x29C,
        0x29D,
        0x29F,
        0x2A1,
        0x2A2,
        0x2B0,
        0x2B2,
        0x2B7,
        0x2BC,
        0x2C8,
        0x2CC,
        0x2D0,
        0x2D1,
        0x2D4,
        0x2D5,
        0x2D6,
        0x2D7,
        0x2DE,
        0x2E0,
        0x2E1,
        0x2E3,
        0x2E4,
        0x2E5,
        0x2E6,
        0x2E7,
        0x2E8,
        0x2E9,
        0x300,
        0x301,
        0x302,
        0x303,
        0x304,
        0x306,
        0x308,
        0x30A,
        0x30B,
        0x30C,
        0x30D,
        0x30F,
        0x311,
        0x318,
        0x319,
        0x31A,
        0x31C,
        0x31D,
        0x31E,
        0x31F,
        0x320,
        0x324,
        0x325,
        0x329,
        0x32A,
        0x32C,
        0x32F,
        0x330,
        0x334,
        0x339,
        0x33A,
        0x33B,
        0x33C,
        0x33D,
        0x346,
        0x351,
        0x357,
        0x361,
        0x3B2,
        0x3B8,
        0x3C7,
        0x1D4A,
        0x1D91,
        0x1DBF,
        0x2016,
        0x203F,
        0x207F,
        0x2197,
        0x2198,
        0x2C71,
        0xA71B,
        0xA71C
    ];

    /// <summary>
    /// Returns real length of the given string.
    /// Correctly counts even characters occupying more than two bytes.
    /// </summary>
    /// <param name="theString">The string whose length is to be returned.</param>
    /// <returns>Real length of the given string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the given string is null.</exception>
    public static int GetStringRealLength(string theString) =>
        new StringInfo(theString).LengthInTextElements;

    /// <summary>
    /// Returns the first character of the given string.
    /// Correctly returns the first character even when it occupies more than two bytes.
    /// </summary>
    /// <param name="theString"></param>
    /// <returns>The first character of the given string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the given string is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given string is empty.</exception>
    public static string GetStringFirstCharacterAsString(string theString) =>
        new StringInfo(theString).SubstringByTextElements(0, 1);

    /// <summary>
    /// Decides if the given string contains only Chinese characters.
    /// A whitespace is not considered to be a Chinese character.
    /// I.e., if the string contains a whitespace, false is returned regardless other characters.
    /// </summary>
    /// <param name="theString">The string to be checked.</param>
    /// <returns>True when the given string contains only Chinese characters, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the given string is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given string is empty.</exception>
    public static bool IsOnlyChineseCharacters(string theString)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(theString, string.Empty);

        var stringInfo = new StringInfo(theString);

        for (var i = 0; i < stringInfo.LengthInTextElements; i++)
        {
            var singleCharacter = stringInfo.SubstringByTextElements(i, 1);
            var singleCharacterCode = char.ConvertToUtf32(singleCharacter, 0);

            if (!_chineseUnicodeRanges.Any(range => range.Lower <= singleCharacterCode && range.Upper >= singleCharacterCode))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Decides if the given string contains only IPA characters.
    /// A whitespace is not considered to be an IPA character.
    /// I.e., if the string contains a whitespace, false is returned regardless other characters.
    /// </summary>
    /// <param name="theString">The string to be checked.</param>
    /// <returns>True when the given string contains only IPA characters, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the given string is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given string is empty.</exception>
    public static bool IsOnlyIpaCharacters(string theString)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(theString, string.Empty);

        var stringInfo = new StringInfo(theString);

        for (var i = 0; i < stringInfo.LengthInTextElements; i++)
        {
            var singleCharacter = stringInfo.SubstringByTextElements(i, 1);
            var singleCharacterCode = char.ConvertToUtf32(singleCharacter, 0);

            if (!_ipaSymbolCodes.Any(ipaSymbolCode => ipaSymbolCode == singleCharacterCode))
            {
                return false;
            }
        }

        return true;
    }
}
