namespace AsciiPinyin.Web.Server.Constants;

internal static class Errors
{
    public const string ALTERNATIVE_ALREADY_EXISTS = "combination 'alternative character + character + pinyin + tone' already exists";
    public const string CHACHAR_ALREADY_EXISTS = "combination 'character + pinyin + tone' already exists";
    public const string EMPTY = "value is empty";
    public const string MISSING = "value is missing";
    public const string NO_ASCII = "contains non-ASCII characters";
    public const string NO_IPA = "contains non-IPA characters";
    public const string NO_RADICAL = "combination 'character + pinyin + tone' is not radical";
    public const string NO_SINGLE_CHINESE = "not a chinese character";
    public const string ONE_TO_NINETY_NINE = "outside allowed range from 1 to 99";
    public const string ONLY_ONE_CHARACTER_ALLOWED = "only one character allowed";
    public const string UNKNOWN_ALTERNATIVE = "unknown combination 'alternative character + character + pinyin + tone'";
    public const string UNKNOWN_CHACHAR = "unknown combination 'character + pinyin + tone'";
    public const string USER_AGENT_MISSING = "User-Agent missing in the request header";
    public const string ZERO_TO_FOUR = "outside allowed range from 0 to 4";
}
