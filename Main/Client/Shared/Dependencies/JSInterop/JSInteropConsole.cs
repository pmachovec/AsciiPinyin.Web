using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Shared.Dependencies.JSInterop;

public class JSInteropConsole : IJSInteropConsole
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropConsole(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async void ConsoleWarning(string warningText)
    {
        await _jsRuntime.InvokeVoidAsync("consoleWarning", warningText);
    }

    public async void ConsoleError(string errorText)
    {
        await _jsRuntime.InvokeVoidAsync("errorWarning", errorText);
    }
}
