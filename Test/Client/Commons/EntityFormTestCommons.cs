using AsciiPinyin.Web.Client.ComponentInterfaces;
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
    IRenderedComponent<IBackdrop> _backdropComponent,
    BunitJSInterop _jsInterop,
    Mock<IEntityClient> _entityClientMock,
    Mock<IIndex> _indexMock,
    string _entityFormRootId,
    string _backdropRootId
) where T : IEntity
{
    private const string DIV = "div";

    public async Task OpenTest(JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var modalRoot = _entityFormComponent.Find($"#{_entityFormRootId}");
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));

        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");
        Assert.That(backdropRoot, Is.Not.Null);
        Assert.That(backdropRoot.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));
        Assert.That(_entityFormComponent.Instance.Backdrop, Is.Null);

        await _entityFormComponent.Instance.OpenAsync(_indexMock.Object, CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));

        var backdrop = _entityFormComponent.Instance.Backdrop;
        Assert.That(backdrop, Is.Not.Null);
        Assert.That(backdrop!.ZIndex, Is.EqualTo(1));
        Assert.That(backdropRoot.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(backdropRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

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

    public void NumberInputUnchangedTest(
        short? theInput,
        string inputId
    )
    {
        _ = _jsInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, inputId).SetResult(true);
        InputValueSetTest(theInput, theInput, inputId);
    }

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

    public async Task SubmitInvalidInputTest(
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
        await formSubmitButton.ClickAsync(new());

        Assert.That(formInput.ClassList, Does.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    public async Task SubmitInvalidButtonInputTest(
        string expectedError,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        await formSubmitButton.ClickAsync(new());

        Assert.That(formInput.ClassList, Does.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.Find($"#{validationMessageId}");
        Assert.That(validationMessageElement!.InnerHtml, Is.Not.Null);
        Assert.That(validationMessageElement.InnerHtml, Is.EqualTo(expectedError));
    }

    public async Task SubmitValidInputTest(
        string inputValue,
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        formInput.Change(inputValue);
        await formSubmitButton.ClickAsync(new());

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
    }

    public async Task SubmitValidButtonInputTest(
        string inputId,
        string validationMessageId,
        string submitButtonId
    )
    {
        var formInput = _entityFormComponent.Find($"#{inputId}");
        var formSubmitButton = _entityFormComponent.Find($"#{submitButtonId}");
        await formSubmitButton.ClickAsync(new());

        Assert.That(formInput.ClassList, Does.Not.Contain(CssClasses.INVALID));

        var validationMessageElement = _entityFormComponent.FindAll($"#{validationMessageId}");
        Assert.That(validationMessageElement, Is.Empty);
    }

    public void MockPostStatusCode(Alternative alternative, HttpStatusCode statusCode) =>
        MockPostStatusCode(alternative, ApiNames.ALTERNATIVES, statusCode);

    public void MockPostStatusCode(Chachar chachar, HttpStatusCode statusCode) =>
        MockPostStatusCode(chachar, ApiNames.CHARACTERS, statusCode);

    public async Task ClickFirstInSelector(
        string selectorInputId,
        string selectorCssClass,
        JSRuntimeInvocationHandler setTitleHandler,
        JSRuntimeInvocationHandler setAfterClickTitleHandler,
        string expectedTitle,
        string expectedAfterClickTitle,
        int afterClickCalledTimes = 2
    )
    {
        // Open the selector
        var openRadicalSelectorButton = _entityFormComponent.Find($"#{selectorInputId}");
        var backdrop = _entityFormComponent.Instance.Backdrop;
        Assert.That(openRadicalSelectorButton, Is.Not.Null);
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        Assert.That(backdrop, Is.Not.Null);
        Assert.That(backdrop!.ZIndex, Is.EqualTo(1));
        await openRadicalSelectorButton.ClickAsync(new());
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        Assert.That(backdrop.ZIndex, Is.EqualTo(2));

        // Click on the first button in the selector.
        // Mocking of proper clicking to a concrete button is too complicated and not worth the struggle.
        var buttonDivs = _entityFormComponent.FindAll(DIV).Where(div => div.ClassList.Contains(selectorCssClass));
        var firstButtonDiv = buttonDivs.FirstOrDefault();
        Assert.That(firstButtonDiv, Is.Not.Null);

        if (afterClickCalledTimes == 1)
        {
            setAfterClickTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedAfterClickTitle);
        }
        else
        {
            _ = setAfterClickTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, afterClickCalledTimes - 1, expectedAfterClickTitle);
        }

        await firstButtonDiv!.ClickAsync(new());
        _ = setAfterClickTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, afterClickCalledTimes, expectedAfterClickTitle);
        Assert.That(backdrop.ZIndex, Is.EqualTo(1));
    }

    private void MockPostStatusCode<T1>(T1 entity, string apiName, HttpStatusCode statusCode) where T1 : IEntity =>
        _entityClientMock
            .Setup(entityClient => entityClient.PostEntityAsync(apiName, entity, It.IsAny<CancellationToken>()))
            .Returns(Task.Run(() => statusCode));
}
