using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Shared.JSInterop;

/// <summary>
/// Methods for calling JavaScript methods manipulating with DOM.
/// </summary>
public class JSInteropDOM
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropDOM(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Sets title of the page.
    /// </summary>
    /// <param name="title">The title to be set.</param>
    public async void SetTitle(string title)
    {
        await _jsRuntime.InvokeVoidAsync("setTitle", title);
    }
}
