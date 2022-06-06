namespace AsciiPinyin.Web.Client.Shared.Dependencies;

/// <summary>
/// Methods for calling JavaScript methods writing to the browser console.
/// </summary>
public interface IJSInteropConsole
{
    /// <summary>
    /// Write a warning to the console.
    /// </summary>
    /// <param name="warningText">The text to be written as a warning to the console.</param>
    void ConsoleWarning(string warningText);

    /// <summary>
    /// Write an error to the console.
    /// </summary>
    /// <param name="errorText">The text to be written as an error to the console.</param>
    void ConsoleError(string errorText);
}
