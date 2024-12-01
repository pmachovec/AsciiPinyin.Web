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
    /// <returns></returns>
    Task EnableAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the 'disabled' JavaScript property of the given element to true.
    /// </summary>
    /// <param name="elementId">ID of the element to be disabled.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task DisableAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the given CSS class for the given element.
    /// If the element already has the class, nothing happens.
    /// </summary>
    /// <param name="elementId">ID of the element for which the class should be added.</param>
    /// <param name="theClass">The name of the class.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task AddClassAsync(string elementId, string theClass, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the given CSS class from the given element.
    /// If the element already doesn't have the class, nothing happens.
    /// </summary>
    /// <param name="elementId">ID of the element from which the class should be removed.</param>
    /// <param name="theClass">The name of the class.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task RemoveClassAsync(string elementId, string theClass, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given text to the given element.
    /// </summary>
    /// <param name="elementId">ID of the elemet for to which the text should be set.</param>
    /// <param name="text">The text to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task SetTextAsync(string elementId, string text, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given value to the given element.
    /// </summary>
    /// <param name="elementId">ID of the elemet for to which the value should be set.</param>
    /// <param name="value">The value to be set.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task SetValueAsync(string elementId, string value, CancellationToken cancellationToken);

    /// <summary>
    /// Decides if the given element contains an invalid input value by the EcmaScript definition of 'validity.badInput'.
    /// </summary>
    /// <param name="elementId">The element whose input value is to be checked.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
    Task<bool> IsValidInputAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the text of the given element to null.
    /// </summary>
    /// <param name="elementId">ID of the element for which the text should be set to null.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns></returns>
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
    /// <returns></returns>
    Task SetZIndexAsync(string elementId, int value, CancellationToken cancellationToken);

    /// <summary>
    /// Removes all but "d-none" display css classes from the given element.
    /// Adds "d-none" css class to the given element.
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Display2NoneAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes "d-none" css class from the given element, if the element has the class.
    /// Adds "d-block" css class to the given element.
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task None2BlockAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes "d-none" css class from the given element, if the element has the class.
    /// Adds "d-flex" css class to the given element.
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task None2FlexAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the 'bg-primary' css class for the given element.
    /// If the element already has the class, nothing happens.
    /// If the element has other 'bg' classes, they are removed from the element
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetBgPrimaryAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the 'bg-warning' css class for the given element.
    /// If the element already has the class, nothing happens.
    /// If the element has other 'bg' classes, they are removed from the element
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetBgWarningAsync(string elementId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the 'bg-danger' css class for the given element.
    /// If the element already has the class, nothing happens.
    /// If the element has other 'bg' classes, they are removed from the element
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetBgDangerAsync(string elementId, CancellationToken cancellationToken);
}
