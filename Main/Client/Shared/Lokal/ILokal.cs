namespace AsciiPinyin.Web.Client.Shared.Lokal;

/// <summary>
/// String properties containing localized strings based on currently selected culture.
/// </summary>
public interface ILokal
{
    string Alternatives { get; }
    string Ascii { get; }
    string AsciiPinyin { get; }
    string BaseCharacter { get; }
    string BaseCharacterAsciiPinyin { get; }
    string Characters { get; }
    string Ipa { get; }
    string Pinyin { get; }
    string NumberOfStrokes { get; }
}
