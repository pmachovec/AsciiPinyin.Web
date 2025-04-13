using AngleSharp.Diffing.Extensions;
using AngleSharp.Dom;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityViewDialogTestCommons(
    IRenderedComponent<IComponent> _viewDialogComponent,
    IRenderedComponent<IComponent> _processDialogComponent,
    string viewDialogDeleteTooltipId
)
{
    public void DeleteButtonDisabledTest(string expectedTooltipStart, params string[] expectedTooltipParts)
    {
        var deleteButtonTooltipElement = _viewDialogComponent.Find($"#{viewDialogDeleteTooltipId}");
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

        var deleteButtonClassAttribute = DeleteButtonClassTest(deleteButton!);
        Assert.That(deleteButtonClassAttribute!.Value, Does.Contain(CssClasses.DISABLED));
        Assert.That(deleteButtonClassAttribute!.Value, Does.Contain(CssClasses.OPACITY_25));
    }

    public async Task DeleteEntityTest()
    {
        var deleteButtonTooltipElement = _viewDialogComponent.Find($"#{viewDialogDeleteTooltipId}");
        var deleteButtonTooltip = DeleteButtonTooltipTest(deleteButtonTooltipElement);
        Assert.That(deleteButtonTooltip, Is.Empty);

        var deleteButton = deleteButtonTooltipElement.FirstElementChild;
        Assert.That(deleteButton, Is.Not.Null);

        var deleteButtonClassAttribute = DeleteButtonClassTest(deleteButton!);
        Assert.That(deleteButtonClassAttribute!.Value, Does.Not.Contain(CssClasses.DISABLED));
        Assert.That(deleteButtonClassAttribute!.Value, Does.Not.Contain(CssClasses.OPACITY_25));

        await deleteButton!.ClickAsync(new());

        var proceedButton = _processDialogComponent.Find($"#{IDs.PROCESS_DIALOG_BUTTON_PROCEED}");
        Assert.That(proceedButton, Is.Not.Null);

        await proceedButton.ClickAsync(new());
    }

    private static string DeleteButtonTooltipTest(IElement? deleteButtonTooltipElement)
    {
        Assert.That(deleteButtonTooltipElement, Is.Not.Null);
        Assert.That(deleteButtonTooltipElement!.TryGetAttr(Attributes.TITLE, out var deleteButtonTooltipTilteAttribute), Is.True);
        Assert.That(deleteButtonTooltipTilteAttribute, Is.Not.Null);

        return deleteButtonTooltipTilteAttribute!.Value;
    }

    private static IAttr DeleteButtonClassTest(IElement deleteButton)
    {
        Assert.That(deleteButton.TryGetAttr(Attributes.CLASS, out var deleteButtonClassAttribute), Is.True);
        Assert.That(deleteButtonClassAttribute, Is.Not.Null);

        return deleteButtonClassAttribute!;
    }
}
