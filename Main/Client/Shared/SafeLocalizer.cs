using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Shared;

/// <summary>
/// Localization methods with safe measurements against not found translations.
/// </summary>
public class SafeLocalizer
{
    private readonly IStringLocalizer<Resource> _localizer;
    private readonly JSInteropConsole _jsIteropConsole;

    public SafeLocalizer(
        IStringLocalizer<Resource> localizer,
        JSInteropConsole jsInteropConsole)
    {
        _localizer = localizer;
        _jsIteropConsole = jsInteropConsole;
    }

    /// <summary>
    /// Returns a localization for the given key. If not found, writes an error to the console and uses a default invariant localization
    /// for the given key. If not found, writes another error to the console and returns the key itself.
    /// </summary>
    /// <param name="key">The key to find a localization for.</param>
    public string GetString(string key)
    {
        if (_localizer[key] == null)
        {
            var callerNameForConsole = new StackFrame(1).GetMethod()?.DeclaringType?.Name;
            _jsIteropConsole.ConsoleError($"Localized string for key '{key}' not found with culture '{CultureInfo.CurrentCulture.Name}' in '{callerNameForConsole}'.");
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
            _jsIteropConsole.ConsoleError($"Invariant value for key '{key}' not found with with invariant culture '{CultureInfo.InvariantCulture.Name}' in {callerNameForConsole}.");
            return key;
        }

        return invariantString;
    }
}
