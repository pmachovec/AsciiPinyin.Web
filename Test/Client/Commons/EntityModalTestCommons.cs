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
    public async Task OpenTest(T entity, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalRootId}");
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertHidden(modalRoot);

        await _entityModalComponent.Instance.OpenAsync(entity, _indexMock.Object, CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(modalRoot);
    }

    public async Task CloseTest(JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalRootId}");
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(modalRoot);

        await _entityModalComponent.Instance.CloseAsync(CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertHidden(modalRoot);
    }

    public async Task ClickProcessDialogBackButtonTest(JSRuntimeInvocationHandler setTitleHandler, string expectedTitle, int calledTimes = 2)
    {
        var processDialogButtonBack = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_BACK}");
        Assert.That(processDialogButtonBack, Is.Not.Null);

        if (calledTimes == 1)
        {
            setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        }
        else
        {
            _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, calledTimes - 1, expectedTitle);
        }

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, calledTimes - 1, expectedTitle);
        await processDialogButtonBack.ClickAsync(new());
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, calledTimes, expectedTitle);
    }

    public async Task ClickProcessDialogProceedButtonTest(JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var processDialogButtonProceed = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(processDialogButtonProceed, Is.Not.Null);

        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        await processDialogButtonProceed.ClickAsync(new());
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
    }

    public void ProcessDialogWarningTest(
        string expectedMessageTemplate,
        params string[] expectedMessageParams
    )
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");
        var expectedMessage = string.Format(CultureInfo.InvariantCulture, expectedMessageTemplate, expectedMessageParams);

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

    public void ProcessDialogErrorTest(string expectedMessage)
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");

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

    public void ProcessDialogSuccessTest(string expectedMessageTemplate, params string[] expectedMessageParams)
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogBody = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BODY}");
        var expectedMessage = string.Format(CultureInfo.InvariantCulture, expectedMessageTemplate, expectedMessageParams);

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

    public void ProcessDialogOverModalClosedTest(string modalId)
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var modalRoot = _entityModalComponent.Find($"#{modalId}");
        AssertHidden(processDialogRoot);
        AssertVisible(modalRoot);
    }

    public void ModalClosedTest(string modalId)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalId}");
        Assert.That(modalRoot, Is.Not.Null);
        AssertHidden(modalRoot);
    }

    public void TitlesOrderTest(params string[] expectedTitles)
    {
        var invocations = _jsInterop.Invocations.Where(i => i.Identifier == DOMFunctions.SET_TITLE);
        Assert.That(invocations.Count(), Is.EqualTo(expectedTitles.Length));

        for (var i = 0; i < expectedTitles.Length; i++)
        {
            var args = invocations.ElementAt(i).Arguments;
            Assert.That(args.Count, Is.EqualTo(1));

            var name = args.ElementAt(0);
            Assert.That(name, Is.InstanceOf<string>());
            Assert.That(name, Is.EqualTo(expectedTitles[i]));
        }
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
