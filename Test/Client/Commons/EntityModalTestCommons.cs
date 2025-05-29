using AngleSharp.Dom;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;
using System.Globalization;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityModalTestCommons<T>
(
    IRenderedComponent<IEntityModal<T>> _entityModalComponent,
    IRenderedComponent<IComponent> _processDialogComponent,
    BunitJSInterop _jsInterop,
    Mock<IIndex> _indexMock,
    string modalRootId
) where T : IEntity
{
    public async Task OpenTest(T entity, string title) =>
        await OpenTest(
            entity,
            _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, title).SetVoidResult(),
            title
        );

    public async Task OpenTest(T entity, JSRuntimeInvocationHandler setTitleHandler, string title)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalRootId}");
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, title);
        AssertHidden(modalRoot);

        await _entityModalComponent.Instance.OpenAsync(entity, _indexMock.Object, CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, title);
        AssertVisible(modalRoot);
    }

    public async Task CloseTest(string indexTitle)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalRootId}");
        var setIndexTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, indexTitle).SetVoidResult();
        setIndexTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, indexTitle);
        AssertVisible(modalRoot);

        await _entityModalComponent.Instance.CloseAsync(CancellationToken.None);

        _ = setIndexTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, indexTitle);
        AssertHidden(modalRoot);
    }

    public async Task ClickProcessDialogBackButtonTest()
    {
        var processDialogButtonBack = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_BACK}");
        Assert.That(processDialogButtonBack, Is.Not.Null);
        await processDialogButtonBack.ClickAsync(new());
    }

    public async Task ClickProcessDialogProceedButtonTest()
    {
        var processDialogButtonProceed = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(processDialogButtonProceed, Is.Not.Null);
        await processDialogButtonProceed.ClickAsync(new());
    }

    public void ProcessDialogWarningTest(
        JSRuntimeInvocationHandler setWarningTitleHandler,
        string expectedTitle,
        string expectedMessageTemplate,
        params string[] expectedMessageParams
    )
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");
        var expectedMessage = string.Format(CultureInfo.InvariantCulture, expectedMessageTemplate, expectedMessageParams);

        _ = setWarningTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(processDialogRoot);
        Assert.That(processDialogHeader, Is.Not.Null);
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.D_NONE));
        Assert.That(processDialogBody, Is.Not.Null);
        Assert.That(processDialogBody.TextContent, Is.EqualTo(expectedMessage));
    }

    public void ProcessDialogErrorTest(
        JSRuntimeInvocationHandler setErrorTitleHandler,
        string expectedTitle,
        string expectedMessage
    )
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");

        _ = setErrorTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(processDialogRoot);
        Assert.That(processDialogHeader, Is.Not.Null);
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.D_NONE));
        Assert.That(processDialogBody, Is.Not.Null);
        Assert.That(processDialogBody.TextContent, Is.EqualTo(expectedMessage));
    }

    public void ProcessDialogSuccessTest(
        JSRuntimeInvocationHandler setSuccessTitleHandler,
        string expectedTitle,
        string expectedMessageTemplate,
        params string[] expectedMessageParams
    )
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");
        var expectedMessage = string.Format(CultureInfo.InvariantCulture, expectedMessageTemplate, expectedMessageParams);

        _ = setSuccessTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(processDialogRoot);
        Assert.That(processDialogHeader, Is.Not.Null);
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader.ClassList, Does.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader.ClassList, Does.Not.Contain(CssClasses.D_NONE));
        Assert.That(processDialogBody, Is.Not.Null);
        Assert.That(processDialogBody.TextContent, Is.EqualTo(expectedMessage));
    }

    public void ProcessDialogOverModalClosedTest(JSRuntimeInvocationHandler setTitleHandler, string modalId, string modalTitle, int calledTimes = 2)
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var modalRoot = _entityModalComponent.Find($"#{modalId}");

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, calledTimes, modalTitle);
        AssertHidden(processDialogRoot);
        AssertVisible(modalRoot);
    }

    public void ModalClosedTest(JSRuntimeInvocationHandler setIndexTitleHandler, string modalId, string indexTitle)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalId}");

        _ = setIndexTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, indexTitle);
        Assert.That(modalRoot, Is.Not.Null);
        AssertHidden(modalRoot);
    }

    private static void AssertVisible(IElement? modalRoot)
    {
        Assert.That(modalRoot, Is.Not.Null);
        Assert.That(modalRoot!.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    private static void AssertHidden(IElement? modalRoot)
    {
        Assert.That(modalRoot, Is.Not.Null);
        Assert.That(modalRoot!.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));
    }
}
