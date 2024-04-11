using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.JSInterop;

internal sealed class JSInteropDOM(IJSRuntime jsRuntime) : IJSInteropDOM
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task ShowElementAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("showElement", cancellationToken, elementId);

    public async Task HideElementAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("hideElement", cancellationToken, elementId);

    public async Task EnableAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("enable", cancellationToken, elementId);

    public async Task DisableAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("disable", cancellationToken, elementId);

    public async Task AddClassAsync(string elementId, string theClass, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("addClass", cancellationToken, elementId, theClass);

    public async Task RemoveClassAsync(string elementId, string theClass, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("removeClass", cancellationToken, elementId, theClass);

    public async Task SetTitleAsync(string title, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("setTitle", cancellationToken, title);

    public async Task SetZIndexAsync(string elementId, int value, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync("setZIndex", cancellationToken, elementId, value);
}
