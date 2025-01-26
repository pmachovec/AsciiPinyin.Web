using AngleSharp.Dom;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;
using System.Net;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityFormTestCommons(
    TestContext _testContext,
    IRenderedComponent<IComponent> _formComponent,
    Mock<IEntityClient> _entityClientMock,
    Mock<ISubmitDialog> _submitDialogMock,
    IEnumerable<string> _inputIds
)
{
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
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, inputValue);
        var formInput = _formComponent.Find($"#{inputId}");
        formInput.Input(inputValue);

        setValueInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_VALUE);
        _ = setValueInvocationHandler.SetVoidResult();
    }

    /// <summary>
    /// Tests if a number input, when it's given a value, changes the value to the one that was in it before
    /// - if an 'oninput' event replaces the given value with the previous one.
    /// The actual value given to the input really is not among method parameters, universal string placeholder is used.
    /// </summary>
    /// <param name="previousValidInput">Previous value of the input.</param>
    /// <param name="inputId">ID of the input.</param>
    public void NumberInputAdjustedTest(
        string previousValidInput,
        string inputId
    )
    {
        // Mock the input to be valid first.
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, previousValidInput);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        var formNumberInput = _formComponent.Find($"#{inputId}");
        formNumberInput.Input(previousValidInput);

        // Now mock invalid input and verify that it was changed to the previous valid one.
        // Substitutes all invalid inputs, no need to run the test for each one separately.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(false);
        VerifyInputValueSet(It.IsAny<string>(), previousValidInput, inputId);
    }

    /// <summary>
    /// Tests if a number input, when it's given a value, keeps the value unchanged
    /// - if an eventual 'oninput' event does not alter the value.
    /// </summary>
    /// <param name="theInput">Value given to the input</param>
    /// <param name="inputId">ID of the input</param>
    public void NumberInputUnchangedTest(
        string theInput,
        string inputId
    )
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        VerifyInputValueSet(theInput, theInput, inputId);
    }

    public void VerifyInputValueSet(
        string valueToSet,
        string expectedContent,
        string inputId
    )
    {
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, expectedContent);
        var chacharFormInput = _formComponent.Find($"#{inputId}");
        chacharFormInput.Input(valueToSet);

        var setValueInvocation = setValueInvocationHandler.VerifyInvoke(DOMFunctions.SET_VALUE);
        Assert.That(setValueInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setValueInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(setValueInvocation.Arguments[1], Is.EqualTo(expectedContent));
        _ = setValueInvocationHandler.SetVoidResult();
    }

    /// <summary>
    /// Tests behavior of an input when it contains an invalid value and the form is submitted.
    /// The value of the input is processed and assigned to a variable of the form by the 'oninput' callback.
    /// </summary>
    /// <param name="inputValue">Invalid value in the input</param>
    /// <param name="expectedError">Expected error message to appear in the error label</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    public void WrongSubmitOnInputTest(
        string inputValue,
        string expectedError,
        string inputId,
        string submitButtonId,
        string errorDivId
    )
    {
        var formInput = _formComponent.Find($"#{inputId}");
        WrongSubmitTest(formInput.Input, inputValue, expectedError, inputId, submitButtonId, errorDivId);
    }

    /// <summary>
    /// Tests behavior of an input when it contains an invalid value and the form is submitted.
    /// The value of the input is assigned to a variable of the form by the 'bind-value' attribute.
    /// </summary>
    /// <param name="inputValue">Invalid value in the input</param>
    /// <param name="expectedError">Expected error message to appear in the error label</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    public void WrongSubmitOnChangeTest(
        string inputValue,
        string expectedError,
        string inputId,
        string submitButtonId,
        string errorDivId
    )
    {
        var formInput = _formComponent.Find($"#{inputId}");
        WrongSubmitTest(formInput.Change, inputValue, expectedError, inputId, submitButtonId, errorDivId);
    }

    /// <summary>
    /// Tests behavior of an input when it contains a valid value and the form is submitted.
    /// The value of the input is processed and assigned to a variable of the form by the 'oninput' callback.
    /// </summary>
    /// <param name="inputValue">Valid value in the input</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    public void CorrectSubmitOnInputTest(
        string inputValue,
        string inputId,
        string submitButtonId,
        string errorDivId
    )
    {
        var formInput = _formComponent.Find($"#{inputId}");
        CorrectSubmitTest(formInput.Input, inputValue, inputId, submitButtonId, errorDivId);
    }

    /// <summary>
    /// Tests behavior of an input when it contains a valid value and the form is submitted.
    /// The value of the input is assigned to a variable of the form by the 'bind-value' attribute.
    /// </summary>
    /// <param name="inputValue">Valid value in the input</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    public void CorrectSubmitOnChangeTest(
       string inputValue,
       string inputId,
       string submitButtonId,
       string errorDivId
    )
    {
        var formInput = _formComponent.Find($"#{inputId}");
        CorrectSubmitTest(formInput.Change, inputValue, inputId, submitButtonId, errorDivId);
    }

    /// <summary>
    /// Mocks setting the danger border for all other inputs than the one corresponding to the given ID.
    /// </summary>
    /// <param name="inputId">ID of the input</param>
    public void MockOtherInputsBorderDanger(string inputId)
    {
        foreach (var otherInputId in _inputIds)
        {
            if (otherInputId != inputId)
            {
                _ = _testContext.JSInterop.SetupVoid(
                    DOMFunctions.ADD_CLASS,
                    otherInputId,
                    CssClasses.BORDER_DANGER
                );
            }
        }
    }

    /// <summary>
    /// Mocks form elements for testing one concrete input behavior and returns their handlers.
    /// </summary>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    /// <param name="expectedError">Expected error message to appear in the error label</param>
    /// <returns>Add danger border css class handler, set error text handler, form submit button handler</returns>
    public (JSRuntimeInvocationHandler, JSRuntimeInvocationHandler, IElement) MockInputFormElements(
        string inputId,
        string submitButtonId,
        string errorDivId,
        string expectedError = ""
    )
    {
        var addBorderDangerClassHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.ADD_CLASS,
            inputId,
            CssClasses.BORDER_DANGER
        );

        var setErrorTextHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_TEXT,
            errorDivId,
            expectedError
        );

        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");

        return (
            addBorderDangerClassHandler,
            setErrorTextHandler,
            formSubmitButton
        );
    }

    public void MockPostStatusCode<T>(T entity, HttpStatusCode statusCode) where T : IEntity
    {
        var apiName = (entity is Alternative) ? ApiNames.ALTERNATIVES : ApiNames.CHARACTERS;

        _ = _entityClientMock
            .Setup(entityClient => entityClient.PostEntityAsync(apiName, entity, It.IsAny<CancellationToken>()))
            .Returns(Task.Run(() => statusCode));
    }

    public void CaptureErrorMessage(Action<string?> captureErrorMessage)
    {
        _ = _submitDialogMock
            .Setup(submitDialog =>
                submitDialog.SetErrorAsync(
                    (IEntityForm)_formComponent.Instance, // Yes, the cast is necessary and it works.
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback((IModal _, string errorMessage, CancellationToken _) => captureErrorMessage(errorMessage));
    }

    public void CaptureSuccessMessage(Action<string?> captureSuccessMessage)
    {
        _ = _submitDialogMock
            .Setup(submitDialog =>
                submitDialog.SetSuccessAsync(
                    (IEntityForm)_formComponent.Instance, // Yes, the cast is necessary and it works.
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback((IModal _, string successMessage, CancellationToken _) => captureSuccessMessage(successMessage));
    }

    /// <summary>
    /// Verifies if the form input reacted correctly when it contained an invalid value after the form was submitted.
    /// </summary>
    /// <param name="expectedError">Expected error message to appear in the error label</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="errorDivId">ID of the error label div</param>
    /// <param name="addBorderDangerClassHandler">Add danger border css class handler corresponding to the input</param>
    /// <param name="setErrorTextHandler">Set error text handler corresponding to the input</param>
    public static void WrongSubmitVerifications(
        string expectedError,
        string inputId,
        string errorDivId,
        JSRuntimeInvocationHandler addBorderDangerClassHandler,
        JSRuntimeInvocationHandler setErrorTextHandler
    )
    {
        var addClassInvocation = addBorderDangerClassHandler.VerifyInvoke(DOMFunctions.ADD_CLASS);
        Assert.That(addClassInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(addClassInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(addClassInvocation.Arguments[1], Is.EqualTo(CssClasses.BORDER_DANGER));

        var setTextInvocation = setErrorTextHandler.VerifyInvoke(DOMFunctions.SET_TEXT);
        Assert.That(setTextInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setTextInvocation.Arguments[0], Is.EqualTo(errorDivId));
        Assert.That(setTextInvocation.Arguments[1], Is.EqualTo(expectedError));
        _ = addBorderDangerClassHandler.SetVoidResult();
        _ = setErrorTextHandler.SetVoidResult();
    }

    /// <summary>
    /// Verifies if the form input reacted correctly when it contained a valid value after the form was submitted.
    /// </summary>
    /// <param name="addBorderDangerClassHandler">Add danger border css class handler corresponding to the input</param>
    /// <param name="setErrorTextInvocationHandler">Set error text handler corresponding to the input</param>
    public static void CorrectSubmitVerifications(
        JSRuntimeInvocationHandler addBorderDangerClassHandler,
        JSRuntimeInvocationHandler setErrorTextInvocationHandler
    )
    {
        addBorderDangerClassHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);
        setErrorTextInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_TEXT);
        _ = addBorderDangerClassHandler.SetVoidResult();
        _ = setErrorTextInvocationHandler.SetVoidResult();
    }

    public void SubmitDialogErrorMessageTest(string? errorMessage, string expectedErrorMessage)
    {
        Assert.That(errorMessage, Is.Not.Null);
        Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));

        _submitDialogMock.Verify(submitDialog =>
            submitDialog.SetErrorAsync(
                (IEntityForm)_formComponent.Instance, // Yes, the cast is necessary and it works.
                expectedErrorMessage,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    public void SubmitDialogSuccessMessageTest(string? successMessage, string expectedSuccessMessage)
    {
        Assert.That(successMessage, Is.Not.Null);
        Assert.That(successMessage, Is.EqualTo(expectedSuccessMessage));

        _submitDialogMock.Verify(submitDialog =>
            submitDialog.SetSuccessAsync(
                (IEntityForm)_formComponent.Instance, // Yes, the cast is necessary and it works.
                expectedSuccessMessage,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    private void WrongSubmitTest(
        Action<string> inputMethod,
        string inputValue,
        string expectedError,
        string inputId,
        string submitButtonId,
        string errorDivId
    )
    {
        var (addBorderDangerClassHandler, setErrorTextInvocationHandler, formSubmitButton) = MockInputFormElements(
            inputId,
            submitButtonId,
            errorDivId,
            expectedError
        );

        inputMethod(inputValue);
        formSubmitButton.Click();

        WrongSubmitVerifications(
            expectedError,
            inputId,
            errorDivId,
            addBorderDangerClassHandler,
            setErrorTextInvocationHandler
        );
    }

    private void CorrectSubmitTest(
        Action<string> inputMethod,
        string inputValue,
        string inputId,
        string submitButtonId,
        string errorDivId
    )
    {
        MockOtherInputsBorderDanger(inputId);

        var (addBorderDangerClassHandler, setErrorTextInvocationHandler, formSubmitButton) = MockInputFormElements(
            inputId,
            submitButtonId,
            errorDivId
        );

        inputMethod(inputValue);
        formSubmitButton.Click();

        CorrectSubmitVerifications(addBorderDangerClassHandler, setErrorTextInvocationHandler);
    }
}
