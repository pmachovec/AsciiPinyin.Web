namespace AsciiPinyin.Web.Client.JSInterop;

/// <summary>
/// Methods for calling JavaScript methods manipulating with DOM.
/// </summary>
public interface IJSInteropDOM
{
    /// <summary>
    /// Sets title of the page.
    /// </summary>
    /// <param name="title">The title to be set.</param>
    void SetTitle(string title);

    /// <summary>
    /// Sets element visibility to none.
    /// </summary>
    /// <param name="elementId">ID of the element to hide.</param>
    void HideElement(string elementId);

    /// <summary>
    /// Sets element visibility to block.
    /// </summary>
    /// <param name="elementId">ID of the element to show.</param>
    void ShowElement(string elementId);
}
