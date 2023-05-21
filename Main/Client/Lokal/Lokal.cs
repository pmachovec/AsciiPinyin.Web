using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Resources;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Lokal;

/// <summary>
/// Localization logic for <see cref="ILokal"/> properties, contains safe measurements against not found translations.
/// </summary>
internal class Lokal : ILokal
{
    private readonly IJSInteropConsole _jsIteropConsole;
    private readonly IStringLocalizer<Resource> _localizer;

    public string Alternatives => GetSafeString("Alternatives");
    public string Ascii => GetSafeString("ASCII");
    public string AsciiPinyin => GetSafeString("ASCIIPinyin");
    public string BaseCharacter => GetSafeString("BaseCharacter");
    public string BaseCharacterAsciiPinyin => GetSafeString("BaseCharacterASCIIPinyin");
    public string Characters => GetSafeString("Characters");
    public string Ipa => GetSafeString("IPA");
    public string Pinyin => GetSafeString("Pinyin");
    public string NumberOfStrokes => GetSafeString("NumberOfStrokes");

    public Lokal(
        IJSInteropConsole jsInteropConsole,
        IStringLocalizer<Resource> localizer)
    {
        _jsIteropConsole = jsInteropConsole;
        _localizer = localizer;
    }

    /// <summary>
    /// Returns a localization for the given key. If not found, writes an error to the console and uses a default invariant localization
    /// for the given key. If not found, writes another error to the console and returns the key itself.
    /// </summary>
    /// <param name="key">The key to find a localization for.</param>
    private string GetSafeString(string key)
    {
        if (_localizer[key] == null)
        {
            var callerNameForConsole = new StackFrame(1).GetMethod()?.DeclaringType?.Name;
            _jsIteropConsole.ConsoleError($"Localized string for key '{key}' not found with culture '{CultureInfo.CurrentCulture.Name}' in '{callerNameForConsole}', using invariant string.");
            return GetInvariantString(key, callerNameForConsole);
        }

        return _localizer[key];
    }

    /// <summary>
    /// Returns a default invariant localization for the given key. If not found, writes error to the console and returns the key itself.
    /// </summary>
    /// <param name="key">The key to find an invariant localization for.</param>
    /// <param name="callerNameForConsole">Name of the caller to be mentioned in warning messages in the console.</param>
    private string GetInvariantString(string key, string? callerNameForConsole)
    {
        var invariantString = Resource.ResourceManager.GetString(key, CultureInfo.InvariantCulture);

        if (invariantString == null)
        {
            _jsIteropConsole.ConsoleError($"Invariant string for key '{key}' not found with with invariant culture '{CultureInfo.InvariantCulture.Name}' in {callerNameForConsole}, using the key itself.");
            return key;
        }

        return invariantString;
    }
}
