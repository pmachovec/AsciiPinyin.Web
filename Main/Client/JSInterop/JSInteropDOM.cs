using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Constants.JSInterop;
using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.JSInterop;

public sealed class JSInteropDOM(IJSRuntime jsRuntime) : IJSInteropDOM
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task ShowElementAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.SHOW_ELEMENT, cancellationToken, elementId);

    public async Task HideElementAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.HIDE_ELEMENT, cancellationToken, elementId);

    public async Task EnableAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.ENABLE, cancellationToken, elementId);

    public async Task DisableAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.DISABLE, cancellationToken, elementId);

    public async Task AddClassAsync(string elementId, string theClass, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.ADD_CLASS, cancellationToken, elementId, theClass);

    public async Task RemoveClassAsync(string elementId, string theClass, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.REMOVE_CLASS, cancellationToken, elementId, theClass);

    public async Task SetTitleAsync(string title, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.SET_TITLE, cancellationToken, title);

    public async Task SetTextAsync(string elementId, string text, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.SET_TEXT, cancellationToken, elementId, text);

    public async Task SetValueAsync(string elementId, string value, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.SET_VALUE, cancellationToken, elementId, value);

    public async Task<bool> IsValidInputAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeAsync<bool>(DOMFunctions.IS_VALID_INPUT, cancellationToken, elementId);

    public async Task RemoveTextAsync(string elementId, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.REMOVE_TEXT, cancellationToken, elementId);

    public async Task SetZIndexAsync(string elementId, int value, CancellationToken cancellationToken) =>
        await _jsRuntime.InvokeVoidAsync(DOMFunctions.SET_Z_INDEX, cancellationToken, elementId, value);

    public async Task Block2NoneAsync(string elementId, CancellationToken cancellationToken) =>
        await Task.WhenAll(
            RemoveClassAsync(elementId, CssClasses.D_BLOCK, cancellationToken),
            AddClassAsync(elementId, CssClasses.D_NONE, cancellationToken)
        );

    public async Task None2BlockAsync(string elementId, CancellationToken cancellationToken) =>
        await Task.WhenAll(
            RemoveClassAsync(elementId, CssClasses.D_NONE, cancellationToken),
            AddClassAsync(elementId, CssClasses.D_BLOCK, cancellationToken)
        );
}
