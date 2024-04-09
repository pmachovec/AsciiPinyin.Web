using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.JSInterop;

internal sealed class JSInteropDOM(IJSRuntime jsRuntime) : IJSInteropDOM
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async void SetTitle(string title) => await _jsRuntime.InvokeVoidAsync("setTitle", title);

    public async void HideElement(string elementId) => await _jsRuntime.InvokeVoidAsync("hideElement", elementId);

    public async void ShowElement(string elementId) => await _jsRuntime.InvokeVoidAsync("showElement", elementId);
}
