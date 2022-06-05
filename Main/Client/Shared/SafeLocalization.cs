using AsciiPinyin.Web.Client.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Shared;

/// <summary>
/// Localization methods with safe measurements against not found translations.
/// </summary>
public class SafeLocalization
{
    private readonly IStringLocalizer<Resource> _localizer;

    public SafeLocalization(IStringLocalizer<Resource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Returns a localization for the given key. If not found, writes a warning to the console and uses a default invariant localization
    /// for the given key. If not found, writes another warning to the console and returns the key itself.
    /// </summary>
    /// <param name="key">The key to find a localization for.</param>
    /// <param name="callerNameForConsole">Name of the caller to be mentioned in warning messages in the console.</param>
    public string GetLocalizedString(string key, string callerNameForConsole)
    {
        if (_localizer[key] == null)
        {
            Console.WriteLine($"SafeLocalizationSingleton.GetLocalizedString: Localized value for '{key}' not found in {callerNameForConsole}.");
            return GetInvariantString(key, callerNameForConsole);
        }

        return _localizer[key];
    }

    /// <summary>
    /// Returns a default invariant localization for the given key. If not found, writes another warning to the console and returns
    /// the key itself.
    /// </summary>
    /// <param name="key">The key to find an invariant localization for.</param>
    /// <param name="callerNameForConsole">Name of the caller to be mentioned in warning messages in the console.</param>
    private static string GetInvariantString(string key, string callerNameForConsole)
    {
        var invariantString = Resource.ResourceManager.GetString(key, CultureInfo.InvariantCulture);

        if (invariantString == null)
        {
            Console.WriteLine($"SafeLocalizationSingleton.GetInvariantString: Invariant value for '{key}' not found in {callerNameForConsole}.");
            return key;
        }

        return invariantString;
    }
}
