namespace AsciiPinyin.Web.Client.JSInterop;

/// <summary>
/// Methods for calling JavaScript methods manipulating with DOM.
/// </summary>
public interface IJSInteropDOM
{
    /// <summary>
    /// Sets element visibility to block.
    /// </summary>
    /// <param name="elementId">ID of the element to show.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task ShowElementAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets element visibility to none.
    /// </summary>
    /// <param name="elementId">ID of the element to hide.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task HideElementAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the 'disabled' JavaScript property of the given element to false.
    /// </summary>
    /// <param name="elementId">ID of the element to be enabled.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task EnableAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the 'disabled' JavaScript property of the given element to true.
    /// </summary>
    /// <param name="elementId">ID of the element to be disabled.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task DisableAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets text content of the given element.
    /// </summary>
    /// <param name="elementId">ID of the element whose text should be got.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>Text content of the element.</returns>
    Task<string> GetTextAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given text to the given element.
    /// </summary>
    /// <param name="elementId">ID of the elemet for which the text should be set.</param>
    /// <param name="text">The text to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task SetTextAsync(string elementId, string text, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given value to the given element.
    /// </summary>
    /// <param name="elementId">ID of the elemet for which the value should be set.</param>
    /// <param name="value">The value to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task SetValueAsync<T>(string elementId, T value, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given value to the given attribute of the given element.
    /// </summary>
    /// <param name="elementId">ID of the element for which the attribute value should be set.</param>
    /// <param name="attributeName">Name of the attribute for which the value should be set.</param>
    /// <param name="value">The value to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task SetAttributeAsync(string elementId, string attributeName, string value, CancellationToken cancellationToken);

    /// <summary>
    /// Decides if the given element contains a valid input value by the EcmaScript definition of 'validity.badInput'.
    /// </summary>
    /// <param name="elementId">The element whose input value is to be checked.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>True if the element contains a valid input value, false otherwise.</returns>
    Task<bool> IsValidInputAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the text of the given element to null.
    /// </summary>
    /// <param name="elementId">ID of the element for which the text should be set to null.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task RemoveTextAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets title of the page.
    /// </summary>
    /// <param name="title">The title to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task SetTitleAsync(string title, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the Z index of the element to the given value.
    /// </summary>
    /// <param name="elementId">ID of the element to set the Z index to.</param>
    /// <param name="value">The value of the set Z index.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    Task SetZIndexAsync(string elementId, int value, CancellationToken cancellationToken);
}
