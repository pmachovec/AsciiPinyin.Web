using AngleSharp.Dom;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Constants.JSInterop;
using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityFormTestCommons(
    TestContext _testContext,
    IRenderedComponent<IComponent> _formComponent,
    IEnumerable<string> _inputIds)
{
    public void StringInputUnchangedTest(
        string theInput,
        string inputId)
    {
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, theInput);
        var formInput = _formComponent.Find($"#{inputId}");
        formInput.Input(theInput);

        setValueInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_VALUE);
        _ = setValueInvocationHandler.SetVoidResult();
    }

    public void NumberInputAdjustedTest(
        string previousValidInput,
        string inputId)
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

    public void NumberInputUnchangedTest(
        string theInput,
        string inputId)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        VerifyInputValueSet(theInput, theInput, inputId);
    }

    public void VerifyInputValueSet(
        string valueToSet,
        string expectedContent,
        string inputId)
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

    public void WrongSubmitOnInputTest(
        string theInput,
        string expectedError,
        string inputId,
        string submitButtonId,
        string errorDivId)
    {
        var (addBorderDangerClassHandler, setErrorTextInvocationHandler, formInput, formSubmitButton) = MockFormElements(
            inputId,
            submitButtonId,
            errorDivId,
            expectedError);

        formInput.Input(theInput);
        formSubmitButton.Click();

        WrongSubmitTest(
            expectedError,
            inputId,
            errorDivId,
            addBorderDangerClassHandler,
            setErrorTextInvocationHandler);
    }

    public void WrongSubmitOnChangeTest(
        string theInput,
        string expectedError,
        string inputId,
        string submitButtonId,
        string errorDivId)
    {
        var (addBorderDangerClassHandler, setErrorTextInvocationHandler, formInput, formSubmitButton) = MockFormElements(
            inputId,
            submitButtonId,
            errorDivId,
            expectedError);

        formInput.Change(theInput);
        formSubmitButton.Click();

        WrongSubmitTest(
            expectedError,
            inputId,
            errorDivId,
            addBorderDangerClassHandler,
            setErrorTextInvocationHandler);
    }

    public void CorrectSubmitOnInputTest(
        string theInput,
        string inputId,
        string submitButtonId,
        string errorDivId)
    {
        MockOtherInputsBorderDanger(inputId);

        var (addBorderDangerClassHandler, setTextInvocationHandler, formInput, chacharFormSubmitButton) = MockFormElements(
            inputId,
            submitButtonId,
            errorDivId);

        formInput.Input(theInput);
        chacharFormSubmitButton.Click();

        CorrectSubmitTest(addBorderDangerClassHandler, setTextInvocationHandler);
    }

    public void CorrectSubmitOnChangeTest(
       string theInput,
       string inputId,
       string submitButtonId,
       string errorDivId)
    {
        MockOtherInputsBorderDanger(inputId);

        var (addBorderDangerClassHandler, setErrorTextInvocationHandler, formInput, chacharFormSubmitButton) = MockFormElements(
            inputId,
            submitButtonId,
            errorDivId);

        formInput.Change(theInput);
        chacharFormSubmitButton.Click();

        CorrectSubmitTest(addBorderDangerClassHandler, setErrorTextInvocationHandler);
    }

    public void MockOtherInputsBorderDanger(string inputId)
    {
        foreach (var otherInputId in _inputIds)
        {
            if (otherInputId != inputId)
            {
                _ = _testContext.JSInterop.SetupVoid(
                    DOMFunctions.ADD_CLASS,
                    otherInputId,
                    CssClasses.BORDER_DANGER);
            }
        }
    }

    public (JSRuntimeInvocationHandler, JSRuntimeInvocationHandler, IElement, IElement) MockFormElements(
        string inputId,
        string submitButtonId,
        string errorDivId,
        string expectedError = "")
    {
        var addBorderDangerClassHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.ADD_CLASS,
            inputId,
            CssClasses.BORDER_DANGER);
        var setErrorTextHandler = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_TEXT,
            errorDivId,
            expectedError);
        var formInput = _formComponent.Find($"#{inputId}");
        var formSubmitButton = _formComponent.Find($"#{submitButtonId}");

        return (
            addBorderDangerClassHandler,
            setErrorTextHandler,
            formInput,
            formSubmitButton);
    }

    public static void WrongSubmitTest(
        string expectedError,
        string inputId,
        string errorDivId,
        JSRuntimeInvocationHandler addBorderDangerClassHandler,
        JSRuntimeInvocationHandler setErrorTextHandler)
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

    public static void CorrectSubmitTest(
        JSRuntimeInvocationHandler addBorderDangerClassHandler,
        JSRuntimeInvocationHandler setErrorTextInvocationHandler)
    {
        addBorderDangerClassHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);
        setErrorTextInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_TEXT);
        _ = addBorderDangerClassHandler.SetVoidResult();
        _ = setErrorTextInvocationHandler.SetVoidResult();
    }
}
