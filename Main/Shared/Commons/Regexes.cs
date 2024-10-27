using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Commons;

public static partial class Regexes
{
    [GeneratedRegex("^[a-zA-Z]+$")]
    public static partial Regex AsciiLettersRegex();
}
