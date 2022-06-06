namespace AsciiPinyin.Web.Client.Shared.Dependencies;

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
}
