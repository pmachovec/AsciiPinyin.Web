using AsciiPinyin.Web.Shared.Resources;
using Microsoft.Extensions.Localization;
using Moq;

namespace Asciipinyin.Web.Client.Test.Tools;

internal sealed class LocalizerMockSetter(Mock<IStringLocalizer<Resource>> _localizerMock)
{
    public void SetUpResources(params (string, string)[] resourcesWIthTranslations)
    {
        foreach ((var resource, var translation) in resourcesWIthTranslations)
        {
            _ = _localizerMock
                .Setup(localizer => localizer[resource])
                .Returns(new LocalizedString(translation, translation));
        }
    }
}
