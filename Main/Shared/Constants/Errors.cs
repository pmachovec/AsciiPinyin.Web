namespace AsciiPinyin.Web.Shared.Constants;

public static class Errors
{
    public const string ALTERNATIVE_EXISTS = "alternative already exists";
    public const string ALTERNATIVE_UNKNOWN = "alternative unknown";
    public const string CHACHAR_EXISTS = "chachar already exists";
    public const string CHACHAR_UNKNOWN = "chachar unknown";
    public const string EMPTY = "value is empty";
    public const string HAS_ALTERNATIVES = "has existing alternatives";
    public const string IS_ALTERNATIVE_FOR_CHACHARS = "is alternative for existing chachars";
    public const string IS_RADICAL_FOR_OTHERS = "is radical for other existing chachars";
    public const string MISSING = "value is missing";
    public const string NO_ASCII = "contains non-ASCII characters";
    public const string NO_IPA = "contains non-IPA characters";
    public const string NO_SINGLE_CHINESE = "not a chinese character";
    public const string ONE_TO_NINETY_NINE = "outside allowed range from 1 to 99";
    public const string ONLY_ONE_CHARACTER_ALLOWED = "only one character allowed";
    public const string ORIGINAL_NOT_RADICAL = "original not radical";
    public const string ORIGINAL_UNKNOWN = "original unknown";
    public const string RADICAL_NOT_RADICAL = "radical not radical";
    public const string RADICAL_UNKNOWN = "radical unknown";
    public const string USER_AGENT_MISSING = "User-Agent missing in the request header";
    public const string ZERO_TO_FOUR = "outside allowed range from 0 to 4";
}
