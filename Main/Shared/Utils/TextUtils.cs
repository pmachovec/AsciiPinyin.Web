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

    public static int GetStringRealLength(string theString) =>
        new StringInfo(theString).LengthInTextElements;

    public static bool IsOnlyChineseCharacters(string theString)
    {
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
}
