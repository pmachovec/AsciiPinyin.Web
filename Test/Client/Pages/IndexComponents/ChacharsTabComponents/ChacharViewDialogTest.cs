using Asciipinyin.Web.Client.Test.Commons;
using Asciipinyin.Web.Client.Test.Constants.JSInterop;
using Asciipinyin.Web.Client.Test.Tools;
using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
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
    private const string IS_RADICAL_FOR_OTHERS = nameof(IS_RADICAL_FOR_OTHERS);
    private const string OK = nameof(OK);
    private const string PROCESSING = nameof(PROCESSING);
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
    private EntityViewDialogTestCommons _entityViewDialogTestCommons = default!;
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
                    _radicalChachar4,
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
            (Resource.OK, OK),
            (Resource.Processing, PROCESSING),
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
            _nonRadicalChacharWithoutAlternative21,
            _nonRadicalChacharWithAlternative31
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
            IDs.CHACHAR_VIEW_DIALOG_DELETE_TOOLTIP,
            Attributes.DATA_BS_ORIGINAL_TITLE,
            string.Empty
        ).SetVoidResult();

        _jsInteropSetter.SetUpSetTitles(
            $"{PROCESSING}...",
            SUCCESS,
            WARNING
        );

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
            _processDialogComponent,
            IDs.CHACHAR_VIEW_DIALOG_DELETE_TOOLTIP
        );
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _testContext.Dispose();

    [Test]
    public async Task OpenRadicalChacharWithAlternativesTest()
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {_radicalChachar1.TheCharacter}");
        await _chacharViewDialogComponent.Instance.OpenAsync(_indexMock.Object, _radicalChachar1, CancellationToken.None);
        _entityViewDialogTestCommons.DeleteButtonDisabledTest(CANNOT_BE_DELETED, ALTERNATIVES_EXIST);
    }

    [Test]
    public async Task OpenRadicalChacharRadicalForOthersTest()
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {_radicalChachar2.TheCharacter}");
        await _chacharViewDialogComponent.Instance.OpenAsync(_indexMock.Object, _radicalChachar2, CancellationToken.None);
        _entityViewDialogTestCommons.DeleteButtonDisabledTest(CANNOT_BE_DELETED, IS_RADICAL_FOR_OTHERS);
    }

    [Test]
    public async Task OpenRadicalChacharWithAlternativesRadicalForOthersTest()
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {_radicalChachar3.TheCharacter}");
        await _chacharViewDialogComponent.Instance.OpenAsync(_indexMock.Object, _radicalChachar3, CancellationToken.None);
        _entityViewDialogTestCommons.DeleteButtonDisabledTest(CANNOT_BE_DELETED, ALTERNATIVES_EXIST, IS_RADICAL_FOR_OTHERS);
    }

    [Test]
    public async Task DeleteRadicalChacharTest() =>
        await DeleteChacharTest(_radicalChachar4);

    [Test]
    public async Task DeleteNonRadicalChacharWithoutAlternativeTest() =>
        await DeleteChacharTest(_nonRadicalChacharWithoutAlternative21);

    [Test]
    public async Task DeleteNonRadicalChacharWthAlternativeTest() =>
        await DeleteChacharTest(_nonRadicalChacharWithAlternative31);

    private async Task DeleteChacharTest(Chachar chachar)
    {
        _jsInteropSetter.SetUpSetTitles($"{StringConstants.ASCII_PINYIN} - {chachar.TheCharacter}");
        await _chacharViewDialogComponent.Instance.OpenAsync(_indexMock.Object, chachar, CancellationToken.None);
        await _entityViewDialogTestCommons.DeleteEntityTest();
        Assert.That(_chachars, Does.Not.Contain(chachar));
    }
}
