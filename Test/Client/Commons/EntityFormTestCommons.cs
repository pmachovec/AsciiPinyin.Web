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
    private const string DIV = "div";

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
    /// </summary>
    /// <param name="previousValidInput">Previous value of the input.</param>
    /// <param name="inputId">ID of the input.</param>
    public void NumberInputAdjustedTest(
        short? previousValidInput,
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
        VerifyInputValueSet(It.IsAny<short>(), previousValidInput, inputId);
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
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        VerifyInputValueSet(theInput, theInput, inputId);
    }

    public void VerifyInputValueSet<T>(
        T valueToSet,
        T expectedContent,
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
    /// Tests behavior of an input and its corresponding ValidationMessage element when the input contains an invalid value and the form is submitted.
    /// The input is expected to be assigned the 'invalid' class by blazor.
    /// </summary>
    /// <param name="inputValue">Invalid value in the input</param>
    /// <param name="expectedError">Expected error message to appear in the ValidationMessage element</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void InvalidInputSubmitTest(
        string inputValue,
        string expectedError,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _formComponent.Find($"#{inputId}");
        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");
        formInput.Change(inputValue);
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Contain(CssClasses.INVALID));

        var validationMessageElement = _formComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    /// <summary>
    /// Tests behavior of a button input and its corresponding ValidationMessage element when the input contains an invalid value and the form is submitted.
    /// The input is expected to be assigned the 'invalid' class by JS interoperability.
    /// </summary>
    /// <param name="expectedError">Expected error message to appear in the ValidationMessage element</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="validationMessageId">ID of the ValidationMessage element for the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    public void InvalidButtonInputSubmitTest(
        string expectedError,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        // When a class is assigned by JS interoperability, its presence cannot be tested with bUnit - the class doesn't appear in the elemen's class list.
        // The only thing possible to be tested is the actuall JS interoperability call.
        var addInvalidClassHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.ADD_CLASS,
            inputId,
            CssClasses.INVALID
        );

        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");
        formSubmitButton.Click();

        var addClassInvocation = addInvalidClassHandler.VerifyInvoke(DOMFunctions.ADD_CLASS);
        Assert.That(addClassInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(addClassInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(addClassInvocation.Arguments[1], Is.EqualTo(CssClasses.INVALID));

        var validationMessageElement = _formComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    /// <summary>
    /// Tests behavior of an input and its corresponding ValidationMessage element when the input contains a valid value and the form is submitted.
    /// The input is expected not to be assigned the 'invalid' class neither by blazor nor by JS interoperability.
    /// </summary>
    /// <param name="inputValue">Valid value in the input</param>
    /// <param name="inputId">ID of the input</param>
    /// <param name="submitButtonId">ID of the submit button of the form</param>
    /// <param name="errorDivId">ID of the error label div</param>
    public void ValidInputSubmitTest(
        string inputValue,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var addInvalidClassHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.ADD_CLASS,
            inputId,
            CssClasses.INVALID
        );

        MockOtherInputsInvalid(inputId);
        var formInput = _formComponent.Find($"#{inputId}");
        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");
        formInput.Change(inputValue);
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));
        addInvalidClassHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);

        var validationMessageElement = _formComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
    }

    public void ValidButtonInputSubmitTest(
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var addInvalidClassHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.ADD_CLASS,
            inputId,
            CssClasses.INVALID
        );

        MockOtherInputsInvalid(inputId);
        var formInput = _formComponent.Find($"#{inputId}");
        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");
        formSubmitButton.Click();

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));
        addInvalidClassHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);

        var validationMessageElement = _formComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
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

    public void ClickFirstInSelector(
        string selectorInputId,
        string selectorCssClass
    )
    {
        // Open the selector
        var openRadicalSelectorButton = _formComponent.Find($"#{selectorInputId}");
        Assert.That(openRadicalSelectorButton, Is.Not.Null);
        openRadicalSelectorButton.Click();

        // Click on the first button in the selector.
        // Entities must be mocked so that the first one in the list is the desired one.
        // Mocking of proper clicking to a concrete button is too complicated and not worth the struggle.
        var buttonDivs = _formComponent.FindAll(DIV).Where(div => div.ClassList.Contains(selectorCssClass));
        var firstButtonDiv = buttonDivs.FirstOrDefault();
        Assert.That(firstButtonDiv, Is.Not.Null);
        firstButtonDiv!.Click();
    }

    /// <summary>
    /// Mocks setting the 'invalid' css class for all other inputs than the one corresponding to the given ID.
    /// </summary>
    /// <param name="inputId">ID of the input</param>
    private void MockOtherInputsInvalid(string inputId)
    {
        foreach (var otherInputId in _inputIds)
        {
            if (otherInputId != inputId)
            {
                _ = _testContext.JSInterop.SetupVoid(
                    DOMFunctions.ADD_CLASS,
                    otherInputId,
                    CssClasses.INVALID
                );
            }
        }
    }
}
