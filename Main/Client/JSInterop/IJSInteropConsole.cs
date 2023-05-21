namespace AsciiPinyin.Web.Client.JSInterop;

/// <summary>
/// Methods for calling JavaScript methods writing to the browser console.
/// </summary>
public interface IJSInteropConsole
{
    /// <summary>
    /// Write an info to the console.
    /// </summary>
    /// <param name="warningText">The text to be written as an info to the console.</param>
    void ConsoleInfo(string infoText);

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
