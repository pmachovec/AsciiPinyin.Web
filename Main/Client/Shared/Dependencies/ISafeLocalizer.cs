namespace AsciiPinyin.Web.Client.Shared.Dependencies;

/// <summary>
/// Localization methods with safe measurements against not found translations.
/// </summary>
public interface ISafeLocalizer
{
    /// <summary>
    /// Returns a localization for the given key. If not found, writes an error to the console and uses a default invariant localization
    /// for the given key. If not found, writes another error to the console and returns the key itself.
    /// </summary>
    /// <param name="key">The key to find a localization for.</param>
    string GetString(string key);
}
