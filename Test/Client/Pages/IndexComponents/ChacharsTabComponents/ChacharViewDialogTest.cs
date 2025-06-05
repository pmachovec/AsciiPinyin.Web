using Asciipinyin.Web.Client.Test.Commons;
using Asciipinyin.Web.Client.Test.Constants.JSInterop;
using Asciipinyin.Web.Client.Test.Tools;
using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
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

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.ChacharsTabComponents;

[TestFixture]
internal sealed class ChacharViewDialogTest : IDisposable
{
    private const string CHARACTER_DELETED = "Character '{0}' - '{1}' deleted";
    private const string CHARACTER_WILL_BE_DELETED = "Character '{0}' - '{1}' will be deleted";

    private const string ALTERNATIVES_EXIST = nameof(ALTERNATIVES_EXIST);
    private const string CANNOT_BE_DELETED = nameof(CANNOT_BE_DELETED);
    private const string ERROR = nameof(ERROR);
    private const string INDEX_TITLE = nameof(INDEX_TITLE);
    private const string IS_RADICAL_FOR_OTHERS = nameof(IS_RADICAL_FOR_OTHERS);
    private const string OK = nameof(OK);
    private const string PROCESSING = nameof(PROCESSING);
    private const string PROCESSING_DOTS = $"{PROCESSING}...";
    private const string PROCESSING_ERROR = nameof(PROCESSING_ERROR);
    private const string SUCCESS = nameof(SUCCESS);
    private const string WARNING = nameof(WARNING);

    private static readonly Chachar _radicalChachar1 = new()
    {
        TheCharacter = "人",
        Pinyin = "ren",
        Ipa = "ɻən",
        Tone = 2,
        Strokes = 2
    };

    private static readonly Chachar _radicalChachar2 = new()
    {
        TheCharacter = "儿",
        Pinyin = "er",
        Ipa = "ɚ",
        Tone = 2,
        Strokes = 2
    };

    private static readonly Chachar _radicalChachar3 = new()
    {
        TheCharacter = "雨",
        Pinyin = "yu",
        Ipa = "y:",
        Tone = 3,
        Strokes = 8
    };

    private static readonly Chachar _radicalChachar4 = new()
    {
        TheCharacter = "木",
        Pinyin = "mu",
        Ipa = "mu",
        Tone = 4,
        Strokes = 4
    };

    private static readonly Chachar _radicalChachar5 = new()
    {
        TheCharacter = "辵",
        Pinyin = "chuo",
        Ipa = "ʈʂʰuɔ",
        Tone = 4,
        Strokes = 7
    };

    private static readonly Alternative _alternative11 = new()
    {
        TheCharacter = "亻",
        OriginalCharacter = _radicalChachar1.TheCharacter,
        OriginalPinyin = _radicalChachar1.Pinyin,
        OriginalTone = _radicalChachar1.Tone,
        Strokes = 2
    };

    private static readonly Alternative _alternative31 = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = _radicalChachar3.TheCharacter,
        OriginalPinyin = _radicalChachar3.Pinyin,
        OriginalTone = _radicalChachar3.Tone,
        Strokes = 8
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative21 = new()
    {
        TheCharacter = "四",
        Pinyin = "si",
        Ipa = "sɹ̩",
        Tone = 4,
        Strokes = 5,
        RadicalCharacter = _radicalChachar2.TheCharacter,
        RadicalPinyin = _radicalChachar2.Pinyin,
        RadicalTone = _radicalChachar2.Tone
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative31 = new()
    {
        TheCharacter = "零",
        Pinyin = "ling",
        Ipa = "liŋ",
        Tone = 2,
        Strokes = 13,
        RadicalCharacter = _radicalChachar3.TheCharacter,
        RadicalPinyin = _radicalChachar3.Pinyin,
        RadicalTone = _radicalChachar3.Tone,
        RadicalAlternativeCharacter = _alternative31.TheCharacter
    };

    private static readonly HashSet<Alternative> _alternatives =
    [
        _alternative11,
        _alternative31
    ];

    private static readonly Mock<IEntityClient> _entityClientMock = new();
    private static readonly Mock<IIndex> _indexMock = new();
    private static readonly Mock<IStringLocalizer<Resource>> _localizerMock = new();

    private static readonly LocalizerMockSetter _localizerMockSetter = new(_localizerMock);

    private HashSet<Chachar> _chachars = default!;
    private EntityModalTestCommons<Chachar> _entityModalTestCommons = default!;
    private EntityViewDialogTestCommons<Chachar> _entityViewDialogTestCommons = default!;
    private IRenderedComponent<ChacharViewDialog> _chacharViewDialogComponent = default!;
    private IRenderedComponent<ProcessDialog> _processDialogComponent = default!;
    private JSInteropSetter _jsInteropSetter = default!;
    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _ = _entityClientMock
            .Setup(
                entityClient => entityClient.PostDeleteEntityAsync(
                    ApiNames.CHARACTERS,
                    _radicalChachar5,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(HttpStatusCode.InternalServerError);

        _ = _entityClientMock
            .Setup(
                entityClient => entityClient.PostDeleteEntityAsync(
                    ApiNames.CHARACTERS,
                    _radicalChachar5,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(HttpStatusCode.OK);

        _ = _entityClientMock
            .Setup(
                entityClient => entityClient.PostDeleteEntityAsync(
                    ApiNames.CHARACTERS,
                    _nonRadicalChacharWithoutAlternative21,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(HttpStatusCode.OK);

        _ = _entityClientMock
            .Setup(
                entityClient => entityClient.PostDeleteEntityAsync(
                    ApiNames.CHARACTERS,
                    _nonRadicalChacharWithAlternative31,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(HttpStatusCode.OK);

        _localizerMockSetter.SetUpResources(
            (Resource.AlternativesExistForCharacter, ALTERNATIVES_EXIST),
            (Resource.CannotBeDeleted, CANNOT_BE_DELETED),
            (Resource.CharacterDeleted, CHARACTER_DELETED),
            (Resource.CharacterWillBeDeleted, CHARACTER_WILL_BE_DELETED),
            (Resource.CharacterIsRadicalForOthers, IS_RADICAL_FOR_OTHERS),
            (Resource.Error, ERROR),
            (Resource.OK, OK),
            (Resource.Processing, PROCESSING),
            (Resource.ProcessingError, PROCESSING_ERROR),
            (Resource.Success, SUCCESS),
            (Resource.Warning, WARNING)
        );
    }

    [SetUp]
    public void SetUp()
    {
        _chachars =
        [
            _radicalChachar1,
            _radicalChachar2,
            _radicalChachar3,
            _radicalChachar4,
            _radicalChachar5,
            _nonRadicalChacharWithoutAlternative21,
            _nonRadicalChacharWithAlternative31
        ];

        _ = _indexMock
            .Setup(index => index.Alternatives)
            .Returns(_alternatives);

        _ = _indexMock
            .Setup(index => index.Chachars)
            .Returns(_chachars);

        _ = _indexMock
            .Setup(index => index.HtmlTitle)
            .Returns(INDEX_TITLE);

        _testContext = new TestContext();
        _jsInteropSetter = new(_testContext.JSInterop);

        _ = _testContext.JSInterop.SetupVoid(BlazorBootstrapFunctions.TOOLTIP_INITIALIZE, _ => true).SetVoidResult();

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_ATTRIBUTE,
            IDs.CHACHAR_VIEW_DIALOG_DELETE_TOOLTIP,
            Attributes.DATA_BS_ORIGINAL_TITLE,
            string.Empty
        ).SetVoidResult();

        _jsInteropSetter.SetUpSetTitles(PROCESSING_DOTS);
        _jsInteropSetter.SetUpSetZIndex(IDs.CHACHAR_VIEW_DIALOG_ROOT);

        _ = _testContext.Services
            .AddSingleton(_entityClientMock.Object)
            .AddSingleton(_localizerMock.Object)
            .AddSingleton<IJSInteropDOM, JSInteropDOM>()
            .AddSingleton<IModalCommons, ModalCommons>();

        _processDialogComponent = _testContext.RenderComponent<ProcessDialog>();

        _ = _indexMock
            .Setup(index => index.ProcessDialog)
            .Returns(_processDialogComponent.Instance);

        _chacharViewDialogComponent = _testContext.RenderComponent<ChacharViewDialog>(
            parameters => parameters.Add(parameter => parameter.Index, _indexMock.Object)
        );

        _entityViewDialogTestCommons = new(
            _chacharViewDialogComponent,
            IDs.CHACHAR_VIEW_DIALOG_DELETE_TOOLTIP
        );

        _entityModalTestCommons = new(
            _chacharViewDialogComponent,
            _processDialogComponent,
            _testContext.JSInterop,
            _indexMock,
            IDs.CHACHAR_VIEW_DIALOG_ROOT
        );
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _testContext.Dispose();

    [Test]
    public async Task OpenCloseRadicalWithAlternativesTest() =>
        await OpenCloseDeleteButtonDisabledTest(_radicalChachar1, ALTERNATIVES_EXIST);

    [Test]
    public async Task OpenCloseRadicalForOthersTest() =>
        await OpenCloseDeleteButtonDisabledTest(_radicalChachar2, IS_RADICAL_FOR_OTHERS);

    [Test]
    public async Task OpenCloseRadicalWithAlternativesRadicalForOthersTest() =>
        await OpenCloseDeleteButtonDisabledTest(_radicalChachar3, ALTERNATIVES_EXIST, IS_RADICAL_FOR_OTHERS);

    [Test]
    public async Task DeleteChacharErrorTest()
    {
        var dialogTitle = $"{StringConstants.ASCII_PINYIN} - {_radicalChachar4.TheCharacter}";
        var setDialogTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, dialogTitle).SetVoidResult();
        var setProcessDialogWarningTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, WARNING).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityModalTestCommons.OpenTest(_radicalChachar4, setDialogTitleHandler, dialogTitle);
        var deleteButton = _entityViewDialogTestCommons.DeleteButtonEnabledTest();
        await _entityViewDialogTestCommons.DeleteButtonClickTest(deleteButton, setProcessDialogWarningTitleHandler, WARNING);

        _entityModalTestCommons.ProcessDialogWarningTest(CHARACTER_WILL_BE_DELETED,
            _radicalChachar4.TheCharacter!,
            _radicalChachar4.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(PROCESSING_ERROR);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setDialogTitleHandler, dialogTitle);
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_VIEW_DIALOG_ROOT);
        await _entityModalTestCommons.CloseTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_chachars, Does.Contain(_radicalChachar4));

        _entityModalTestCommons.TitlesOrderTest(
            PROCESSING_DOTS,
            dialogTitle,
            WARNING,
            PROCESSING_DOTS,
            ERROR,
            dialogTitle,
            INDEX_TITLE
        );
    }

    [Test]
    public async Task DeleteChacharClickBackTest()
    {
        var dialogTitle = $"{StringConstants.ASCII_PINYIN} - {_radicalChachar5.TheCharacter}";
        var setDialogTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, dialogTitle).SetVoidResult();
        var setProcessDialogWarningTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, WARNING).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityModalTestCommons.OpenTest(_radicalChachar5, setDialogTitleHandler, dialogTitle);
        var deleteButton = _entityViewDialogTestCommons.DeleteButtonEnabledTest();
        await _entityViewDialogTestCommons.DeleteButtonClickTest(deleteButton, setProcessDialogWarningTitleHandler, WARNING);

        _entityModalTestCommons.ProcessDialogWarningTest(
            CHARACTER_WILL_BE_DELETED,
            _radicalChachar5.TheCharacter!,
            _radicalChachar5.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setDialogTitleHandler, dialogTitle);
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_VIEW_DIALOG_ROOT);
        await _entityModalTestCommons.CloseTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_chachars, Does.Contain(_radicalChachar5));

        _entityModalTestCommons.TitlesOrderTest(
            PROCESSING_DOTS,
            dialogTitle,
            WARNING,
            dialogTitle,
            INDEX_TITLE
        );
    }

    [Test]
    public async Task DeleteRadicalChacharTest() =>
        await DeleteChacharTest(_radicalChachar5);

    [Test]
    public async Task DeleteNonRadicalChacharWithoutAlternativeTest() =>
        await DeleteChacharTest(_nonRadicalChacharWithoutAlternative21);

    [Test]
    public async Task DeleteNonRadicalChacharWthAlternativeTest() =>
        await DeleteChacharTest(_nonRadicalChacharWithAlternative31);

    private async Task OpenCloseDeleteButtonDisabledTest(Chachar chachar, params string[] expectedTooltipParts)
    {
        var dialogTitle = $"{StringConstants.ASCII_PINYIN} - {chachar.TheCharacter}";
        var setDialogTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, dialogTitle).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityModalTestCommons.OpenTest(chachar, setDialogTitleHandler, dialogTitle);
        _entityViewDialogTestCommons.DeleteButtonDisabledTest(CANNOT_BE_DELETED, expectedTooltipParts);
        await _entityModalTestCommons.CloseTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_chachars, Does.Contain(chachar));

        _entityModalTestCommons.TitlesOrderTest(
            PROCESSING_DOTS,
            dialogTitle,
            INDEX_TITLE
        );
    }

    private async Task DeleteChacharTest(Chachar chachar)
    {
        var dialogTitle = $"{StringConstants.ASCII_PINYIN} - {chachar.TheCharacter}";
        var setDialogTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, dialogTitle).SetVoidResult();
        var setProcessDialogWarningTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, WARNING).SetVoidResult();
        var setProcessDialogSuccessTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SUCCESS).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityModalTestCommons.OpenTest(chachar, setDialogTitleHandler, dialogTitle);
        var deleteButton = _entityViewDialogTestCommons.DeleteButtonEnabledTest();
        await _entityViewDialogTestCommons.DeleteButtonClickTest(deleteButton, setProcessDialogWarningTitleHandler, WARNING);

        _entityModalTestCommons.ProcessDialogWarningTest(
            CHARACTER_WILL_BE_DELETED,
            chachar.TheCharacter!,
            chachar.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setProcessDialogSuccessTitleHandler, SUCCESS);

        _entityModalTestCommons.ProcessDialogSuccessTest(
            CHARACTER_DELETED,
            chachar.TheCharacter!,
            chachar.RealPinyin!
        );

        Assert.That(_chachars, Does.Not.Contain(chachar));
        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setIndexTitleHandler, INDEX_TITLE);
        _entityModalTestCommons.ModalClosedTest(IDs.CHACHAR_VIEW_DIALOG_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            PROCESSING_DOTS,
            dialogTitle,
            WARNING,
            PROCESSING_DOTS,
            SUCCESS,
            INDEX_TITLE
        );
    }
}
