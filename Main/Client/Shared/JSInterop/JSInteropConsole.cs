using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Shared.JSInterop;

/// <summary>
/// Methods for calling JavaScript methods writing to the browser console.
/// </summary>
public class JSInteropConsole
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropConsole(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Write a warning to the console.
    /// </summary>
    /// <param name="warningText">The text to be written as a warning to the console.</param>
    public async void ConsoleWarning(string warningText)
    {
        await _jsRuntime.InvokeVoidAsync("consoleWarning", warningText);
    }

    /// <summary>
    /// Write an error to the console.
    /// </summary>
    /// <param name="errorText">The text to be written as an error to the console.</param>
    public async void ConsoleError(string errorText)
    {
        await _jsRuntime.InvokeVoidAsync("errorWarning", errorText);
    }
}
