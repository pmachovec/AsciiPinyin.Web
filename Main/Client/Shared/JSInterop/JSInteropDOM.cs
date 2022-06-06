using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Shared.JSInterop;

internal class JSInteropDOM : IJSInteropDOM
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropDOM(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async void SetTitle(string title)
    {
        await _jsRuntime.InvokeVoidAsync("setTitle", title);
    }
}
