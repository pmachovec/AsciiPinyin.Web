using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Moq;
using NUnit.Framework;
using System.Net;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityFormTestCommons<T>(
    IRenderedComponent<IEntityForm<T>> _entityFormComponent,
    BunitJSInterop _jsInterop,
    Mock<IEntityClient> _entityClientMock,
    Mock<IIndex> _indexMock,
    string entityFormRootId
) where T : IEntity
{
    private const string DIV = "div";

    /// <summary>
    /// Tests if correct HTML title and CSS classes are set when the form is opened without a specific entity.
    /// </summary>
    /// <param name="title">Expected HTML title</param>
    /// <returns>Task of the asynchronous operation</returns>
    public async Task OpenTest(JSRuntimeInvocationHandler setTitleHandler, string title)
    {
        var modalRoot = _entityFormComponent.Find($"#{entityFormRootId}");
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, title);
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));

        await _entityFormComponent.Instance.OpenAsync(_indexMock.Object, CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, title);
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    /// <summary>
    /// Tests if a string input, when it's given a value, keeps the value unchanged
    /// - if an eventual 'oninput' event does not alter the value.
    /// </summary>
    /// <param name="inputValue">Value given to the input</param>
    /// <param name="inputId">ID of the input</param>
    public void StringInputUnchangedTest(
        string inputValue,
        string inputId
    )
    {
        var setValueInvocationHandler = _jsInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, inputValue);
        var formInput = _entityFormComponent.Find($"#{inputId}");
        formInput.Input(inputValue);

        setValueInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_VALUE);
        _ = setValueInvocationHandler.SetVoidResult();
    }

    /// <summary>
    /// Tests if a number input, when it's given a value, changes the value to the one that was in it before
    /// - if an 'oninput' event replaces the given value with the previous one.
    /// </summary>
    /// <param name="previousValidInput">Previous value of the input.</param>
    /// <param name="inputId">ID of the input.</param>
    public void NumberInputAdjustedTest(
        short? previousValidInput,
        string inputId
    )
    {
        // Mock the input to be valid first.
        _ = _jsInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, previousValidInput);
        _ = _jsInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        var formNumberInput = _entityFormComponent.Find($"#{inputId}");
        formNumberInput.Input(previousValidInput);

        // Now mock invalid input and verify that it was changed to the previous valid one.
        // Substitutes all invalid inputs, no need to run the test for each one separately.
        _ = _jsInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(false);
        InputValueSetTest(It.IsAny<short>(), previousValidInput, inputId);
    }

    /// <summary>
    /// Tests if a number input, when it's given a value, keeps the value unchanged
    /// - if an eventual 'oninput' event does not alter the value.
    /// </summary>
    /// <param name="theInput">Value given to the input</param>
    /// <param name="inputId">ID of the input</param>
    public void NumberInputUnchangedTest(
        short? theInput,
        string inputId
    )
    {
        _ = _jsInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        InputValueSetTest(theInput, theInput, inputId);
    }

    /// <summary>
    /// Sets the given value to the given input and verifies if it matches the ecxpected value afterwards.
    /// There are automatic input value adjustments on some inputs.
    /// </summary>
    /// <typeparam name="T1">Type of the value to set</typeparam>
    /// <param name="valueToSet">The value to set</param>
    /// <param name="expectedContent">Expected input content after the value is set</param>
    /// <param name="inputId">ID of the input</param>
    public void InputValueSetTest<T1>(
        T1 valueToSet,
        T1 expectedContent,
        string inputId
    )
    {
        var setValueInvocationHandler = _jsInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, expectedContent);
        var chacharFormInput = _entityFormComponent.Find($"#{inputId}");
        chacharFormInput.Input(valueToSet);

        var setValueInvocation = setValueInvocationHandler.VerifyInvoke(DOMFunctions.SET_VALUE);
        Assert.That(setValueInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setValueInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(setValueInvocation.Arguments[1], Is.EqualTo(expectedContent));
        _ = setValueInvocationHandler.SetVoidResult();
    }

    /// <summary>
    /// Tests behavior of an input and its corresponding ValidationMessage element when the input contains an invalid value and the form is submitted.
    /// The input is expected to be assigned the 'invalid' class by blazor.
    /// </summary>
    /// <param name="inputValue">Invalid value in the input</param>
    /// <param name="expectedError">Expected error message to appear in the ValidationMessage element</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void SubmitInvalidInputTest(
        string inputValue,
        string expectedError,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        formInput.Change(inputValue);
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    /// <summary>
    /// Tests behavior of a button input and its corresponding ValidationMessage element when the input contains an invalid value and the form is submitted.
    /// The input is expected to be assigned the 'invalid' CSS class.
    /// </summary>
    /// <param name="expectedError">Expected error message to appear in the ValidationMessage element</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void SubmitInvalidButtonInputTest(
        string expectedError,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    /// <summary>
    /// Tests behavior of an input and its corresponding ValidationMessage element when the input contains a valid value and the form is submitted.
    /// The input is expected not to be assigned the 'invalid' class neither by blazor nor by JS interoperability.
    /// </summary>
    /// <param name="inputValue">Valid value in the input</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void SubmitValidInputTest(
        string inputValue,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        formInput.Change(inputValue);
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
    }

    /// <summary>
    /// Tests behavior of a button input and its corresponding ValidationMessage element when the input contains a valid value and the form is submitted.
    /// The input is expected not to be assigned the 'invalid' class neither by blazor nor by JS interoperability.
    /// </summary>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void SubmitValidButtonInputTest(
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
    }

    /// <summary>
    /// Mocks return HTTP status code on the entity client when the given Alternative is posted.
    /// </summary>
    /// <param name="alternative">The alternative to be posted</param>
    /// <param name="statusCode">The HTTP status code to return</param>
    public void MockPostStatusCode(Alternative alternative, HttpStatusCode statusCode) =>
        MockPostStatusCode(alternative, ApiNames.ALTERNATIVES, statusCode);

    /// <summary>
    /// Mocks return HTTP status code on the entity client when the given Chachar is posted.
    /// </summary>
    /// <param name="chachar">The chachar posted</param>
    /// <param name="statusCode">The HTTP status code to return</param>
    public void MockPostStatusCode(Chachar chachar, HttpStatusCode statusCode) =>
        MockPostStatusCode(chachar, ApiNames.CHARACTERS, statusCode);

    /// <summary>
    /// Simulates clicking on the first button in an entity selector.
    /// If used in a scenario where clicking a specific button is needed,
    /// entities must be mocked so that the first one in the list is the desired one.
    /// </summary>
    /// <param name="selectorInputId">ID of the selector input</param>
    /// <param name="selectorCssClass">CSS class of the selector</param>
    public void ClickFirstInSelector(
        string selectorInputId,
        string selectorCssClass
    )
    {
        // Open the selector
        var openRadicalSelectorButton = _entityFormComponent.Find($"#{selectorInputId}");
        Assert.That(openRadicalSelectorButton, Is.Not.Null);
        openRadicalSelectorButton.Click();

        // Click on the first button in the selector.
        // Mocking of proper clicking to a concrete button is too complicated and not worth the struggle.
        var buttonDivs = _entityFormComponent.FindAll(DIV).Where(div => div.ClassList.Contains(selectorCssClass));
        var firstButtonDiv = buttonDivs.FirstOrDefault();
        Assert.That(firstButtonDiv, Is.Not.Null);
        firstButtonDiv!.Click();
    }

    /// <summary>
    /// Mocks return HTTP status code on the entity client when the given entity is posted.
    /// </summary>
    /// <typeparam name="T1">Type of the posted entity</typeparam>
    /// <param name="entity">The entity posted</param>
    /// <param name="apiName">API name corresponding to the entity</param>
    /// <param name="statusCode">The HTTP status code to return</param>
    private void MockPostStatusCode<T1>(T1 entity, string apiName, HttpStatusCode statusCode) where T1 : IEntity =>
        _entityClientMock
            .Setup(entityClient => entityClient.PostEntityAsync(apiName, entity, It.IsAny<CancellationToken>()))
            .Returns(Task.Run(() => statusCode));
}
