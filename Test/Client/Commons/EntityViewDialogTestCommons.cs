using AngleSharp.Diffing.Extensions;
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

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityViewDialogTestCommons<T>(
    IRenderedComponent<IEntityModal<T>> _entityViewDialogComponent,
    IRenderedComponent<IComponent> _processDialogComponent,
    BunitJSInterop _jsInterop,
    Mock<IIndex> _indexMock,
    string entityViewDialogRootId,
    string entityViewDialogDeleteTooltipId
) where T : IEntity
{
    public async Task OpenDialogTest(T entity, string dialogTitle) =>
        await OpenDialogTest(
            entity,
            _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, dialogTitle).SetVoidResult(),
            dialogTitle
        );

    public async Task OpenDialogTest(T entity, JSRuntimeInvocationHandler setDialogTitleHandler, string dialogTitle)
    {
        var dialogRoot = _entityViewDialogComponent.Find($"#{entityViewDialogRootId}");
        setDialogTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, dialogTitle);
        AssertDialogHidden(dialogRoot);

        await _entityViewDialogComponent.Instance.OpenAsync(entity, _indexMock.Object, CancellationToken.None);

        _ = setDialogTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, dialogTitle);
        AssertDialogVisible(dialogRoot);
    }

    public async Task CloseDialogTest(string indexTitle)
    {
        var dialogRoot = _entityViewDialogComponent.Find($"#{entityViewDialogRootId}");
        var setIndexTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, indexTitle).SetVoidResult();
        setIndexTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, indexTitle);
        AssertDialogVisible(dialogRoot);

        await _entityViewDialogComponent.Instance.CloseAsync(CancellationToken.None);

        _ = setIndexTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, indexTitle);
        AssertDialogHidden(dialogRoot);
    }

    public void DeleteButtonDisabledTest(string expectedTooltipStart, params string[] expectedTooltipParts)
    {
        var deleteButtonTooltipElement = _entityViewDialogComponent.Find($"#{entityViewDialogDeleteTooltipId}");
        var deleteButtonTooltip = DeleteButtonTooltipTest(deleteButtonTooltipElement);

        if (expectedTooltipParts.Length == 1)
        {
            Assert.That(deleteButtonTooltip, Is.EqualTo($"{expectedTooltipStart} {expectedTooltipParts.First()}"));
        }
        else
        {
            foreach (var expectedTooltipPart in expectedTooltipParts)
            {
                Assert.That(deleteButtonTooltip, Does.Contain($"{Html.BULLET}{Html.NBSP}{expectedTooltipPart}"));
            }
        }

        var deleteButton = deleteButtonTooltipElement.FirstElementChild;
        Assert.That(deleteButton, Is.Not.Null);
        Assert.That(deleteButton!.ClassList, Does.Contain(CssClasses.DISABLED));
        Assert.That(deleteButton!.ClassList, Does.Contain(CssClasses.OPACITY_25));
    }

    public IElement DeleteButtonEnabledTest()
    {
        var deleteButtonTooltipElement = _entityViewDialogComponent.Find($"#{entityViewDialogDeleteTooltipId}");
        var deleteButtonTooltip = DeleteButtonTooltipTest(deleteButtonTooltipElement);
        Assert.That(deleteButtonTooltip, Is.Empty);

        var deleteButton = deleteButtonTooltipElement.FirstElementChild;
        Assert.That(deleteButton, Is.Not.Null);
        Assert.That(deleteButton!.ClassList, Does.Not.Contain(CssClasses.DISABLED));
        Assert.That(deleteButton!.ClassList, Does.Not.Contain(CssClasses.OPACITY_25));

        return deleteButton;
    }

    public async Task ClickDeleteButtonWarningTest(IElement deleteButton, string processDialogWarningTitle)
    {
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var setProcessDialogWarningTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, processDialogWarningTitle).SetVoidResult();
        setProcessDialogWarningTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, processDialogWarningTitle);

        await deleteButton!.ClickAsync(new());

        _ = setProcessDialogWarningTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, 1, processDialogWarningTitle);
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    public async Task ClickProcessDialogProceedButtonErrorTest(string processDialogErrorTitle)
    {
        var setProcessDialogErrorTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, processDialogErrorTitle).SetVoidResult();
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogButtonProceed = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(processDialogButtonProceed, Is.Not.Null);

        await processDialogButtonProceed.ClickAsync(new());

        _ = setProcessDialogErrorTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, 1, processDialogErrorTitle);
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    public async Task ClickProcessDialogBackButtonTest(JSRuntimeInvocationHandler setDialogTitleHandler, string dialogTitle)
    {
        var dialogRoot = _entityViewDialogComponent.Find($"#{entityViewDialogRootId}");
        var processDialogButtonBack = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_BACK}");
        Assert.That(processDialogButtonBack, Is.Not.Null);

        await processDialogButtonBack.ClickAsync(new());

        _ = setDialogTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, 2, dialogTitle);
        AssertDialogVisible(dialogRoot);
    }

    public async Task ClickProcessDialogProceedButtonTest(string processDialogSuccessTitle)
    {
        var setProcessDialogSuccessTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, processDialogSuccessTitle).SetVoidResult();
        var processDialogHeader = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_HEADER}");
        var processDialogButtonProceed = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(processDialogButtonProceed, Is.Not.Null);

        await processDialogButtonProceed.ClickAsync(new());

        _ = setProcessDialogSuccessTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, processDialogSuccessTitle);
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.D_FLEX));
        Assert.That(processDialogHeader!.ClassList, Does.Contain(CssClasses.BG_PRIMARY));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_DANGER));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.BG_WARNING));
        Assert.That(processDialogHeader!.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    public async Task ClickProcessDialogProceedButtonCloseTest(string indexTitle)
    {
        var setIndexTitleHandler = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, indexTitle).SetVoidResult();
        var dialogRoot = _entityViewDialogComponent.Find($"#{entityViewDialogRootId}");
        var processDialogButtonProceed = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(processDialogButtonProceed, Is.Not.Null);

        await processDialogButtonProceed.ClickAsync(new());

        _ = setIndexTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, indexTitle);
        AssertDialogHidden(dialogRoot);
    }

    private static string DeleteButtonTooltipTest(IElement deleteButtonTooltipElement)
    {
        Assert.That(deleteButtonTooltipElement, Is.Not.Null);
        Assert.That(deleteButtonTooltipElement!.TryGetAttr(Attributes.TITLE, out var deleteButtonTooltipTilteAttribute), Is.True);
        Assert.That(deleteButtonTooltipTilteAttribute, Is.Not.Null);

        return deleteButtonTooltipTilteAttribute!.Value;
    }

    private static void AssertDialogVisible(IElement dialogRoot)
    {
        Assert.That(dialogRoot.ClassList, Does.Contain(CssClasses.D_BLOCK));
        Assert.That(dialogRoot.ClassList, Does.Contain(CssClasses.SHOW));
        Assert.That(dialogRoot.ClassList, Does.Not.Contain(CssClasses.D_NONE));
    }

    private static void AssertDialogHidden(IElement dialogRoot)
    {
        Assert.That(dialogRoot.ClassList, Does.Contain(CssClasses.D_NONE));
        Assert.That(dialogRoot.ClassList, Does.Not.Contain(CssClasses.D_BLOCK));
        Assert.That(dialogRoot.ClassList, Does.Not.Contain(CssClasses.SHOW));
    }
}
