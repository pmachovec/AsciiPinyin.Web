using Asciipinyin.Web.Client.Test.Commons;
using Asciipinyin.Web.Client.Test.Constants.JSInterop;
using Asciipinyin.Web.Client.Test.Tools;
using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Test.Constants;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System.Net;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.AlternativesTabComponents;

internal sealed class AlternativeViewDialogTest : IDisposable
{
    private const string ALTERNATIVE_DELETED = "Alternative '{0}' - '{1}' - '{2}' deleted";
    private const string ALTERNATIVE_WILL_BE_DELETED = "Alternative '{0}' - '{1}' - '{2}' will be deleted";

    private const string CANNOT_BE_DELETED = nameof(CANNOT_BE_DELETED);
    private const string PROCESSING = nameof(PROCESSING);
    private const string SUCCESS = nameof(SUCCESS);
    private const string USED_BY_CHACHARS = nameof(USED_BY_CHACHARS);
    private const string WARNING = nameof(WARNING);

    private static readonly Chachar _radicalChachar1 = new()
    {
        TheCharacter = "雨",
        Pinyin = "yu",
        Ipa = "y:",
        Tone = 3,
        Strokes = 8
    };

    private static readonly Chachar _radicalChachar2 = new()
    {
        TheCharacter = "人",
        Pinyin = "ren",
        Ipa = "ɻən",
        Tone = 2,
        Strokes = 2
    };

    private static readonly Alternative _alternative11 = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = _radicalChachar1.TheCharacter,
        OriginalPinyin = _radicalChachar1.Pinyin,
        OriginalTone = _radicalChachar1.Tone,
    };

    private static readonly Alternative _alternative21 = new()
    {
        TheCharacter = "亻",
        OriginalCharacter = _radicalChachar2.TheCharacter,
        OriginalPinyin = _radicalChachar2.Pinyin,
        OriginalTone = _radicalChachar2.Tone,
        Strokes = 2
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative11 = new()
    {
        TheCharacter = "零",
        Pinyin = "ling",
        Ipa = "liŋ",
        Tone = 2,
        Strokes = 13,
        RadicalCharacter = _radicalChachar1.TheCharacter,
        RadicalPinyin = _radicalChachar1.Pinyin,
        RadicalTone = _radicalChachar1.Tone,
        RadicalAlternativeCharacter = _alternative11.TheCharacter
    };

    private static readonly HashSet<Chachar> _chachars =
    [
        _radicalChachar1,
        _radicalChachar2,
        _nonRadicalChacharWithAlternative11
    ];

    private static readonly Mock<IEntityClient> _entityClientMock = new();
    private static readonly Mock<IIndex> _indexMock = new();
    private static readonly Mock<IStringLocalizer<Resource>> _localizerMock = new();

    private static readonly LocalizerMockSetter _localizerMockSetter = new(_localizerMock);

    private HashSet<Alternative> _alternatives = default!;
    private EntityViewDialogTestCommons _entityViewDialogTestCommons = default!;
    private IRenderedComponent<AlternativeViewDialog> _alternativeViewDialogComponent = default!;
    private IRenderedComponent<ProcessDialog> _processDialogComponent = default!;
    private JSInteropSetter _jsInteropSetter = default!;
    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _ = _entityClientMock
            .Setup(
                entityClient => entityClient.PostDeleteEntityAsync(
                    ApiNames.ALTERNATIVES,
                    _alternative21,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(HttpStatusCode.OK);

        _localizerMockSetter.SetUpResources(
            (Resource.AlternativeUsedByCharactersInDb, USED_BY_CHACHARS),
            (Resource.AlternativeDeleted, ALTERNATIVE_DELETED),
            (Resource.AlternativeWillBeDeleted, ALTERNATIVE_WILL_BE_DELETED),
            (Resource.CannotBeDeleted, CANNOT_BE_DELETED),
            (Resource.Processing, PROCESSING),
            (Resource.Success, SUCCESS),
            (Resource.Warning, WARNING)
        );
    }

    [SetUp]
    public void SetUp()
    {
        _alternatives =
        [
            _alternative11,
            _alternative21
        ];

        _ = _indexMock
            .Setup(index => index.Alternatives)
            .Returns(_alternatives);

        _ = _indexMock
            .Setup(index => index.Chachars)
            .Returns(_chachars);

        _testContext = new TestContext();
        _jsInteropSetter = new(_testContext.JSInterop);

        _ = _testContext.JSInterop.SetupVoid(BlazorBootstrapFunctions.TOOLTIP_INITIALIZE, _ => true).SetVoidResult();

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_ATTRIBUTE,
            IDs.ALTERNATIVE_VIEW_DIALOG_DELETE_TOOLTIP,
            Attributes.DATA_BS_ORIGINAL_TITLE,
            string.Empty
        ).SetVoidResult();

        _jsInteropSetter.SetUpSetTitles(
            $"{PROCESSING}...",
            SUCCESS,
            WARNING
        );

        _jsInteropSetter.SetUpSetZIndex(IDs.ALTERNATIVE_VIEW_DIALOG_ROOT);

        _ = _testContext.Services
            .AddSingleton(_entityClientMock.Object)
            .AddSingleton(_localizerMock.Object)
            .AddSingleton<IJSInteropDOM, JSInteropDOM>()
            .AddSingleton<IModalCommons, ModalCommons>();

        _processDialogComponent = _testContext.RenderComponent<ProcessDialog>();

        _ = _indexMock
            .Setup(index => index.ProcessDialog)
            .Returns(_processDialogComponent.Instance);

        _alternativeViewDialogComponent = _testContext.RenderComponent<AlternativeViewDialog>(
            parameters => parameters.Add(parameter => parameter.Index, _indexMock.Object)
        );

        _entityViewDialogTestCommons = new(
            _alternativeViewDialogComponent,
            _processDialogComponent,
            IDs.ALTERNATIVE_VIEW_DIALOG_DELETE_TOOLTIP
        );
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _testContext.Dispose();

    [Test]
    public async Task OpenUsedByChacharsTest()
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {_alternative11.TheCharacter}");
        await _alternativeViewDialogComponent.Instance.OpenAsync(_indexMock.Object, _alternative11, CancellationToken.None);
        _entityViewDialogTestCommons.DeleteButtonDisabledTest(CANNOT_BE_DELETED, USED_BY_CHACHARS);
    }

    [Test]
    public async Task DeleteAlternativeTest()
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {_alternative21.TheCharacter}");
        await _alternativeViewDialogComponent.Instance.OpenAsync(_indexMock.Object, _alternative21, CancellationToken.None);
        await _entityViewDialogTestCommons.DeleteEntityTest();
        Assert.That(_alternatives, Does.Not.Contain(_alternative21));
    }
}
