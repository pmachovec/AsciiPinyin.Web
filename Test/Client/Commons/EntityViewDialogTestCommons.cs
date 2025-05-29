using AngleSharp.Diffing.Extensions;
using AngleSharp.Dom;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using NUnit.Framework;

namespace Asciipinyin.Web.Client.Test.Commons;

internal sealed class EntityViewDialogTestCommons<T>(
    IRenderedComponent<IEntityModal<T>> _entityViewDialogComponent,
    string entityViewDialogDeleteTooltipId
) where T : IEntity
{
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

    private static string DeleteButtonTooltipTest(IElement deleteButtonTooltipElement)
    {
        Assert.That(deleteButtonTooltipElement, Is.Not.Null);
        Assert.That(deleteButtonTooltipElement!.TryGetAttr(Attributes.TITLE, out var deleteButtonTooltipTilteAttribute), Is.True);
        Assert.That(deleteButtonTooltipTilteAttribute, Is.Not.Null);

        return deleteButtonTooltipTilteAttribute!.Value;
    }
}
