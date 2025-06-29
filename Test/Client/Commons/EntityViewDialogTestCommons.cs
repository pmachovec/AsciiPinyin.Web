using AngleSharp.Diffing.Extensions;
using AngleSharp.Dom;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Moq;
using NUnit.Framework;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityViewDialogTestCommons<T>(
    IRenderedComponent<IEntityViewDialog<T>> _entityViewDialogComponent,
    IRenderedComponent<IBackdrop> _backdropComponent,
    Mock<IIndex> _indexMock,
    string _entityViewDialogDeleteTooltipId,
    string _modalRootId,
    string _backdropRootId
) where T : IEntity
{
    public async Task OpenTest(T entity, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        var modalRoot = _entityViewDialogComponent.Find($"#{_modalRootId}");
        var backdropRoot = _backdropComponent.Find($"#{_backdropRootId}");

        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        ModalTestCommons.AssertHidden(modalRoot);
        ModalTestCommons.AssertBackdropHidden(backdropRoot, _entityViewDialogComponent);

        await _entityViewDialogComponent.Instance.OpenAsync(entity, _indexMock.Object, CancellationToken.None);

        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        ModalTestCommons.AssertVisible(modalRoot);
        ModalTestCommons.AssertBackdropVisible(backdropRoot, _entityViewDialogComponent);
    }

    public void DeleteButtonDisabledTest(string expectedTooltipStart, params string[] expectedTooltipParts)
    {
        var deleteButtonTooltipElement = _entityViewDialogComponent.Find($"#{_entityViewDialogDeleteTooltipId}");
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
        var deleteButtonTooltipElement = _entityViewDialogComponent.Find($"#{_entityViewDialogDeleteTooltipId}");
        var deleteButtonTooltip = DeleteButtonTooltipTest(deleteButtonTooltipElement);
        Assert.That(deleteButtonTooltip, Is.Empty);

        var deleteButton = deleteButtonTooltipElement.FirstElementChild;
        Assert.That(deleteButton, Is.Not.Null);
        Assert.That(deleteButton!.ClassList, Does.Not.Contain(CssClasses.DISABLED));
        Assert.That(deleteButton!.ClassList, Does.Not.Contain(CssClasses.OPACITY_25));

        return deleteButton;
    }

    [
        System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1822:Mark members as static",
            Justification = "With static calls, each would need to explicitly specify generic types. With the current instance approach, generics are fixed per instance."
        ),
        System.Diagnostics.CodeAnalysis.SuppressMessage(
            "CodeQuality",
            "IDE0079:Remove unnecessary suppression",
            Justification = "The CA1822 suppression is marked as unnecessary, which is not true."
    )
    ]
    public async Task DeleteButtonClickTest(IElement deleteButton, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        await deleteButton.ClickAsync(new());
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
    }

    private static string DeleteButtonTooltipTest(IElement deleteButtonTooltipElement)
    {
        Assert.That(deleteButtonTooltipElement, Is.Not.Null);
        Assert.That(deleteButtonTooltipElement!.TryGetAttr(Attributes.TITLE, out var deleteButtonTooltipTilteAttribute), Is.True);
        Assert.That(deleteButtonTooltipTilteAttribute, Is.Not.Null);

        return deleteButtonTooltipTilteAttribute!.Value;
    }
}
