using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.JSInterop;

internal sealed class JSInteropConsole(IJSRuntime jsRuntime) : IJSInteropConsole
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async void ConsoleInfo(string infoText) => await _jsRuntime.InvokeVoidAsync("consoleInfo", infoText);

    public async void ConsoleWarning(string warningText) => await _jsRuntime.InvokeVoidAsync("consoleWarning", warningText);

    public async void ConsoleError(string errorText) => await _jsRuntime.InvokeVoidAsync("errorWarning", errorText);
}
