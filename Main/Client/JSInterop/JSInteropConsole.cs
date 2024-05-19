using AsciiPinyin.Web.Shared.Constants.JSInterop;
using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.JSInterop;

public sealed class JSInteropConsole(IJSRuntime jsRuntime) : IJSInteropConsole
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async void ConsoleInfo(string infoText) => await _jsRuntime.InvokeVoidAsync(ConsoleFunctions.CONSOLE_INFO, infoText);

    public async void ConsoleWarning(string warningText) => await _jsRuntime.InvokeVoidAsync(ConsoleFunctions.CONSOLE_WARNING, warningText);

    public async void ConsoleError(string errorText) => await _jsRuntime.InvokeVoidAsync(ConsoleFunctions.CONSOLE_ERROR, errorText);

    public async void ConsoleError(Exception exception) => await _jsRuntime.InvokeVoidAsync(ConsoleFunctions.CONSOLE_ERROR, exception.ToString());
}
