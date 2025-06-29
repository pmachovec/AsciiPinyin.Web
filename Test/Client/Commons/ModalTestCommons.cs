using AngleSharp.Dom;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using NUnit.Framework;
using System.Globalization;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class ModalTestCommons
(
    IRenderedComponent<IModal> _entityModalComponent,
    IRenderedComponent<IBackdrop> _backdropComponent,
    IRenderedComponent<IModal> _processDialogComponent,
    BunitJSInterop _jsInterop,
    string _modalRootId,
    string _backdropRootId
)
{
    public async Task CloseTest(Func<CancellationToken, Task> modalCloseAsync, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var modalRoot = _entityModalComponent.Find($"#{_modalRootId}");
        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");

        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertVisible(modalRoot);
        AssertBackdropVisible(backdropRoot, _entityModalComponent);

        await modalCloseAsync(CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        AssertHidden(modalRoot);
        AssertBackdropHidden(backdropRoot, _entityModalComponent);
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

        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");
        AssertBackdropVisible(backdropRoot, _processDialogComponent, 2);
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

        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");
        AssertBackdropVisible(backdropRoot, _processDialogComponent, 2);
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

        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");
        AssertBackdropVisible(backdropRoot, _processDialogComponent, 2);
    }

    public void ProcessDialogOverModalClosedTest(string modalId)
    {
        var processDialogRoot = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG}");
        var modalRoot = _entityModalComponent.Find($"#{modalId}");
        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");

        AssertHidden(processDialogRoot);
        AssertVisible(modalRoot);
        Assert.That(_processDialogComponent.Instance.Backdrop, Is.Null);
        AssertBackdropVisible(backdropRoot, _entityModalComponent);
    }

    public void ModalClosedTest(string modalId)
    {
        var modalRoot = _entityModalComponent.Find($"#{modalId}");
        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");

        Assert.That(modalRoot, Is.Not.Null);
        AssertHidden(modalRoot);
        AssertBackdropHidden(backdropRoot, _entityModalComponent);
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

    public static void AssertVisible(IElement? modalRoot)
    {
        Assert.That(modalRoot, Is.Not.Null);
        Assert.That(modalRoot!.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    public static void AssertHidden(IElement? modalRoot)
    {
        Assert.That(modalRoot, Is.Not.Null);
        Assert.That(modalRoot!.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(modalRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));
    }

    public static void AssertBackdropVisible<T1>(IElement? backdropRoot, IRenderedComponent<T1> modalComponent, int expectedZIndex = 1) where T1 : IModal
    {
        Assert.That(backdropRoot, Is.Not.Null);
        Assert.That(backdropRoot!.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(backdropRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
        Assert.That(modalComponent.Instance.Backdrop, Is.Not.Null);
        Assert.That(modalComponent.Instance.Backdrop!.ZIndex, Is.EqualTo(expectedZIndex));
    }

    public static void AssertBackdropHidden<T1>(IElement? backdropRoot, IRenderedComponent<T1> modalComponent) where T1 : IModal
    {
        Assert.That(backdropRoot, Is.Not.Null);
        Assert.That(backdropRoot!.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(backdropRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));
        Assert.That(modalComponent.Instance.Backdrop, Is.Null);
    }
}
