using Asciipinyin.Web.Client.Test.Commons;
using Asciipinyin.Web.Client.Test.Tools;
using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Test.Constants;
using AsciiPinyin.Web.Shared.Utils;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System.Globalization;
using System.Net;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.ChacharsTabComponents;

[TestFixture]
internal sealed class ChacharFormTest : IDisposable
{
    private const string CHARACTER_ALREADY_EXISTS = "Character {0} ({1}) already exists";
    private const string CHARACTER_CREATED = "Character {0} ({1}) created";

    private const string COMPULSORY_VALUE = nameof(COMPULSORY_VALUE);
    private const string CREATE_NEW_CHARACTER = nameof(CREATE_NEW_CHARACTER);
    private const string ERROR = nameof(ERROR);
    private const string INDEX_TITLE = nameof(INDEX_TITLE);
    private const string MUST_BE_CHINESE_CHARACTER = nameof(MUST_BE_CHINESE_CHARACTER);
    private const string ONLY_ASCII_ALLOWED = nameof(ONLY_ASCII_ALLOWED);
    private const string ONLY_IPA_ALLOWED = nameof(ONLY_IPA_ALLOWED);
    private const string PROCESSING = nameof(PROCESSING);
    private const string PROCESSING_DOTS = $"{PROCESSING}...";
    private const string PROCESSING_ERROR = nameof(PROCESSING_ERROR);
    private const string SELECT_RADICAL = nameof(SELECT_RADICAL);
    private const string SELECT_RADICAL_ALTERNATIVE = nameof(SELECT_RADICAL_ALTERNATIVE);
    private const string SUCCESS = nameof(SUCCESS);

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
        TheCharacter = "辵",
        Pinyin = "chuo",
        Ipa = "ʈʂʰuɔ",
        Tone = 4,
        Strokes = 7
    };

    private static readonly Chachar _radicalChachar3 = new()
    {
        TheCharacter = "儿",
        Pinyin = "er",
        Ipa = "ɚ",
        Tone = 2,
        Strokes = 2
    };

    private static readonly Chachar _radicalChachar4 = new()
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
        Strokes = 8
    };

    private static readonly Alternative _alternative21 = new()
    {
        TheCharacter = "⻌",
        OriginalCharacter = _radicalChachar2.TheCharacter,
        OriginalPinyin = _radicalChachar2.Pinyin,
        OriginalTone = _radicalChachar2.Tone,
        Strokes = 3
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative31 = new()
    {
        TheCharacter = "四",
        Pinyin = "si",
        Ipa = "sɹ̩",
        Tone = 4,
        Strokes = 5,
        RadicalCharacter = _radicalChachar3.TheCharacter,
        RadicalPinyin = _radicalChachar3.Pinyin,
        RadicalTone = _radicalChachar3.Tone
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative32 = new()
    {
        TheCharacter = "元",
        Pinyin = "yuan",
        Ipa = "ɥœn",
        Tone = 2,
        Strokes = 4,
        RadicalCharacter = _radicalChachar3.TheCharacter,
        RadicalPinyin = _radicalChachar3.Pinyin,
        RadicalTone = _radicalChachar3.Tone
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

    private static readonly Chachar _nonRadicalChacharWithAlternative12 = new()
    {
        TheCharacter = "雫",
        Pinyin = "na",
        Ipa = "na",
        Tone = 3,
        Strokes = 11,
        RadicalCharacter = _radicalChachar1.TheCharacter,
        RadicalPinyin = _radicalChachar1.Pinyin,
        RadicalTone = _radicalChachar1.Tone,
        RadicalAlternativeCharacter = _alternative11.TheCharacter
    };

    private static readonly HashSet<Alternative> _alternatives = [_alternative11, _alternative21];
    private static readonly MouseEventArgs _mouseEventArgs = new();

    private static readonly Mock<IEntityClient> _entityClientMock = new();
    private static readonly Mock<IIndex> _indexMock = new();
    private static readonly Mock<IStringLocalizer<Resource>> _localizerMock = new();

    private static readonly LocalizerMockSetter _localizerMockSetter = new(_localizerMock);

    private HashSet<Chachar> _chachars = default!;
    private EntityFormTestCommons<Chachar> _entityFormTestCommons = default!;
    private EntityModalTestCommons<Chachar> _entityModalTestCommons = default!;
    private IRenderedComponent<Backdrop> _backdropComponent = default!;
    private IRenderedComponent<ChacharForm> _chacharFormComponent = default!;
    private IRenderedComponent<ProcessDialog> _processDialogComponent = default!;
    private JSInteropSetter _jsInteropSetter = default!;
    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _localizerMockSetter.SetUpResources(
            (Resource.CharacterAlreadyExists, CHARACTER_ALREADY_EXISTS),
            (Resource.CharacterCreated, CHARACTER_CREATED),
            (Resource.CompulsoryValue, COMPULSORY_VALUE),
            (Resource.CreateNewCharacter, CREATE_NEW_CHARACTER),
            (Resource.Error, ERROR),
            (Resource.MustBeChineseCharacter, MUST_BE_CHINESE_CHARACTER),
            (Resource.OnlyAsciiAllowed, ONLY_ASCII_ALLOWED),
            (Resource.OnlyIpaAllowed, ONLY_IPA_ALLOWED),
            (Resource.Processing, PROCESSING),
            (Resource.ProcessingError, PROCESSING_ERROR),
            (Resource.SelectRadical, SELECT_RADICAL),
            (Resource.SelectRadicalAlternative, SELECT_RADICAL_ALTERNATIVE),
            (Resource.Success, SUCCESS)
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
            _nonRadicalChacharWithAlternative11,
            _nonRadicalChacharWithoutAlternative31
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

        _jsInteropSetter.SetUpSetTitles(
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            SELECT_RADICAL,
            SELECT_RADICAL_ALTERNATIVE
        );

        _jsInteropSetter.SetUpSetZIndex(IDs.CHACHAR_FORM_ROOT);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.DISABLE, IDs.CHACHAR_FORM_ALTERNATIVE_INPUT).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.DISABLE, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ENABLE, IDs.CHACHAR_FORM_ALTERNATIVE_INPUT).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ENABLE, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE).SetVoidResult();

        _ = _testContext.Services
            .AddSingleton(_entityClientMock.Object)
            .AddSingleton(_localizerMock.Object)
            .AddSingleton<IEntityFormCommons, EntityFormCommons>()
            .AddSingleton<IJSInteropDOM, JSInteropDOM>()
            .AddSingleton<IModalCommons, ModalCommons>();

        _backdropComponent = _testContext.RenderComponent<Backdrop>(
            parameters => parameters.Add(parameter => parameter.RootId, IDs.INDEX_BACKDROP_ROOT)
        );

        _processDialogComponent = _testContext.RenderComponent<ProcessDialog>();

        _ = _indexMock
            .Setup(index => index.Backdrop)
            .Returns(_backdropComponent.Instance);

        _ = _indexMock
            .Setup(index => index.ProcessDialog)
            .Returns(_processDialogComponent.Instance);

        _chacharFormComponent = _testContext.RenderComponent<ChacharForm>(
            parameters => parameters.Add(parameter => parameter.Index, _indexMock.Object)
        );

        _entityFormTestCommons = new(
            _chacharFormComponent,
            _backdropComponent,
            _testContext.JSInterop,
            _entityClientMock,
            _indexMock,
            IDs.CHACHAR_FORM_ROOT,
            IDs.INDEX_BACKDROP_ROOT
        );

        _entityModalTestCommons = new(
            _chacharFormComponent,
            _backdropComponent,
            _processDialogComponent,
            _testContext.JSInterop,
            _indexMock,
            IDs.CHACHAR_FORM_ROOT,
            IDs.INDEX_BACKDROP_ROOT
        );
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _testContext.Dispose();

    [TestCase("-1", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - single digit negative integer")]
    [TestCase("123", "1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits positive integer")]
    [TestCase("-123", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits negative integer")]
    [TestCase("0.1", "0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits positive float")]
    [TestCase("-0.1", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits negative float")]
    [TestCase("123.456", "1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits positive float")]
    [TestCase("-123.456", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits negative float")]
    [TestCase("   ", " ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple spaces")]
    [TestCase("\n\n\n", "\n", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple new lines")]
    [TestCase("\t\t\t", "\t", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple tabulars")]
    [TestCase(" \n\t", " ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - space, new line and tabular together")]
    [TestCase("{0}", "{", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", "$", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", "a", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", "A", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", "A", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", "T", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", "T", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", "T", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", "T", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", "z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", "Z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", "Z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", "d", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", "d", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", "ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", "Ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", "Ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", "P", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", "P", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", "P", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", "P", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", "д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", "Д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", "Д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", "С", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", "С", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", "С", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", "С", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", "r̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", "ʈ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", "p", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", "p", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", "p", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", "p", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", "p", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", "大", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", "大", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", "大", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", "大", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", "大", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", "𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", "𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", "𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", "𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", "𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", "0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void TheCharacterOnInputAdjustedTest(string inputValue, string expectedContent) =>
        _entityFormTestCommons.InputValueSetTest(inputValue, expectedContent, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT);

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single digit positive integer")]
    [TestCase(" ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - space")]
    [TestCase("\n", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - new line")]
    [TestCase("\t", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - tabluar")]
    [TestCase("\\", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - backslash")]
    [TestCase("\'", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - apostrophe")]
    [TestCase("\"", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - quotes")]
    [TestCase("`", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - backtick")]
    [TestCase(".", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - dot")]
    [TestCase(":", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - colon")]
    [TestCase(";", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - semicolon")]
    [TestCase("@", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - at sign")]
    [TestCase("#", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - hash sign")]
    [TestCase("$", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - dollar sign")]
    [TestCase("{", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - opening curly bracket")]
    [TestCase("}", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - closing curly bracket")]
    [TestCase("a", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single ASCII character lowercase")]
    [TestCase("A", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single ASCII character uppercase")]
    [TestCase("ā", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single pinyin character lowercase")]
    [TestCase("Ā", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single pinyin character uppercase")]
    [TestCase("ř", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("中", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension G")]
    public void TheCharacterOnInputUnchangedTest(string inputValue) =>
        _entityFormTestCommons.StringInputUnchangedTest(inputValue, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - empty string")]
    [TestCase("0", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - zero")]
    [TestCase("1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - space")]
    [TestCase("\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - new line")]
    [TestCase("\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - quotes")]
    [TestCase("`", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - backtick")]
    [TestCase(".", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - dot")]
    [TestCase(":", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - colon")]
    [TestCase(";", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - at sign")]
    [TestCase("#", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task SubmitTheCharacterWrongTest(string inputValue, string expectedError) =>
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.
        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            IDs.CHACHAR_FORM_THE_CHARACTER_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );

    [TestCase("中", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension G")]
    public async Task SubmitTheCharacterCorrectTest(string inputValue)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_VALUE,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            TextUtils.GetStringFirstCharacterAsString(inputValue)
        ).SetVoidResult();

        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            IDs.CHACHAR_FORM_THE_CHARACTER_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - empty string")]
    [TestCase("0", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - zero")]
    [TestCase("1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - space")]
    [TestCase("   ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - backslash")]
    [TestCase("\'", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - apostrophe")]
    [TestCase("\"", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - quotes")]
    [TestCase("`", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - backtick")]
    [TestCase(".", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - dot")]
    [TestCase(":", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - colon")]
    [TestCase(";", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - semicolon")]
    [TestCase("@", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - at sign")]
    [TestCase("#", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - hash sign")]
    [TestCase("$", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - dollar sign")]
    [TestCase("{", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public async Task SubmitPinyinWrongTest(string inputValue, string expectedError) =>
        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_PINYIN_INPUT,
            IDs.CHACHAR_FORM_PINYIN_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );

    [TestCase("a", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinCorrectTest)} - single ASCII character lowercase")]
    [TestCase("A", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinCorrectTest)} - single ASCII character uppercase")]
    [TestCase("abc", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinCorrectTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinCorrectTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitPinyinCorrectTest)} - multiple ASCII characters case combination")]
    public async Task SubmitPinyinCorrectTest(string inputValue) =>
        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.CHACHAR_FORM_PINYIN_INPUT,
            IDs.CHACHAR_FORM_PINYIN_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - empty string")]
    [TestCase("0", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - zero")]
    [TestCase("1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single digit positive integer")]
    [TestCase("-1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single digit negative integer")]
    [TestCase("123", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - two digits positive float")]
    [TestCase("-0.1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - two digits negative float")]
    [TestCase("123.456", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple digits negative float")]
    [TestCase(" ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - space")]
    [TestCase("   ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple spaces")]
    [TestCase("\n", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - new line")]
    [TestCase("\n\n\n", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple new lines")]
    [TestCase("\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - tabluar")]
    [TestCase("\t\t\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - backslash")]
    [TestCase("\"", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - quotes")]
    [TestCase("`", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - backtick")]
    [TestCase(";", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - semicolon")]
    [TestCase("@", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - at sign")]
    [TestCase("#", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - hash sign")]
    [TestCase("$", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - dollar sign")]
    [TestCase("{", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - opening curly bracket")]
    [TestCase("}", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - closing curly bracket")]
    [TestCase("{0}", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("A", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single ASCII character uppercase")]
    [TestCase("ABC", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("ɼ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public async Task SubmitIpaWrongTest(string inputValue, string expectedError) =>
        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_IPA_INPUT,
            IDs.CHACHAR_FORM_IPA_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );

    [TestCase("\'", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - apostrophe")]
    [TestCase(".", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - dot")]
    [TestCase(":", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - colon")]
    [TestCase("a", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - single ASCII character lowercase")] // Lowercase ASCII characters are valid IPA
    [TestCase("abc", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - multiple ASCII characters lowercase")]
    [TestCase("r̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("r̝r̻̝r̝̊", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitIpaCorrectTest)} - Czech text in IPA")]
    public async Task SubmitIpaCorrectTest(string inputValue) =>
        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.CHACHAR_FORM_IPA_INPUT,
            IDs.CHACHAR_FORM_IPA_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );

    [TestCase(null, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value empty string")]
    [TestCase(0, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value zero")]
    [TestCase(1, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value one")]
    [TestCase(4, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value four")]
    public void ToneOnInputAdjustedTest(short? previousValidInputValue) =>
        _entityFormTestCommons.NumberInputAdjustedTest(previousValidInputValue, IDs.CHACHAR_FORM_TONE_INPUT);

    [TestCase(null, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - empty string")]
    [TestCase(0, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - zero")]
    [TestCase(1, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - one")]
    [TestCase(4, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - four")]
    public void ToneOnInputUnchangedTest(short? inputValue) =>
        _entityFormTestCommons.NumberInputUnchangedTest(inputValue, IDs.CHACHAR_FORM_TONE_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitToneWrongTest)} - empty string")]
    public async Task SubmitToneWrongTest(string inputValue, string expectedError)
    {
        // Empty tone is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_TONE_INPUT,
            IDs.CHACHAR_FORM_TONE_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitToneCorrectTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitToneCorrectTest)} - one")]
    [TestCase("4", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitToneCorrectTest)} - four")]
    public async Task SubmitToneCorrectTest(string inputValue)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.CHACHAR_FORM_TONE_INPUT,
            IDs.CHACHAR_FORM_TONE_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase(null, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value empty string")]
    [TestCase(0, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value zero")]
    [TestCase(1, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value one")]
    [TestCase(9, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value nine")]
    [TestCase(13, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value thirteen")]
    [TestCase(66, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value sixty-six")]
    [TestCase(99, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value ninety-nine")]
    public void StrokesOnInputAdjustedTest(short? previousValidInputValue) =>
        _entityFormTestCommons.NumberInputAdjustedTest(previousValidInputValue, IDs.CHACHAR_FORM_STROKES_INPUT);

    [TestCase(null, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - empty string")]
    [TestCase(0, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - zero")]
    [TestCase(1, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - one")]
    [TestCase(9, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - nine")]
    [TestCase(13, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - thirteen")]
    [TestCase(66, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - sixty-six")]
    [TestCase(99, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - ninety-nine")]
    public void StrokesOnInputUnchangedTest(short? inputValue) =>
        _entityFormTestCommons.NumberInputUnchangedTest(inputValue, IDs.CHACHAR_FORM_STROKES_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesWrongTest)} - empty string")]
    public async Task SubmitStrokesWrongTest(string inputValue, string expectedError)
    {
        // Empty strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventStrokesInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_STROKES_INPUT,
            IDs.CHACHAR_FORM_STROKES_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - one")]
    [TestCase("9", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(ChacharFormTest)}.{nameof(SubmitStrokesCorrectTest)} - ninety-nine")]
    public async Task SubmitStrokesCorrectTest(string inputValue)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.CHACHAR_FORM_STROKES_INPUT,
            IDs.CHACHAR_FORM_STROKES_VALIDATION_MESSAGE,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON
        );
    }

    [Test]
    public async Task OpenCloseTest()
    {
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        await _entityModalTestCommons.CloseTest(setIndexTitleHandler, INDEX_TITLE);
        _entityModalTestCommons.TitlesOrderTest(CREATE_NEW_CHARACTER, INDEX_TITLE);
    }

    [Test]
    public async Task SubmitRadicalChacharAlreadyExistsTest() =>
        await SubmitChacharAlreadyExistsTest(_radicalChachar1);


    [Test]
    public async Task SubmitRadicalChacharAlreadyExistsIpaDiffersTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar1.TheCharacter,
            Pinyin = _radicalChachar1.Pinyin,
            Tone = _radicalChachar1.Tone,
            Ipa = _radicalChachar2.Ipa,
            Strokes = _radicalChachar1.Strokes
        };

        await SubmitRadicalChacharAlreadyExistsNonKeyFieldDiffersTest(_radicalChachar1, radicalChacharClone);
    }

    [Test]
    public async Task SubmitRadicalChacharAlreadyExistsStrokesDifferTest()
    {
        var radicalChacharClone = new Chachar
        {
            TheCharacter = _radicalChachar1.TheCharacter,
            Pinyin = _radicalChachar1.Pinyin,
            Tone = _radicalChachar1.Tone,
            Ipa = _radicalChachar1.Ipa,
            Strokes = (short)(_radicalChachar1.Strokes! + 1)
        };

        await SubmitRadicalChacharAlreadyExistsNonKeyFieldDiffersTest(_radicalChachar1, radicalChacharClone);
    }

    [Test]
    public async Task SubmitNonRadicalChacharWithoutAlternativeAlreadyExistsTest() =>
        await SubmitChacharAlreadyExistsTest(_nonRadicalChacharWithoutAlternative31);


    [Test]
    public async Task SubmitNonRadicalChacharWithAlternativeAlreadyExistsTest() =>
        await SubmitChacharAlreadyExistsTest(_nonRadicalChacharWithAlternative11);

    [Test]
    public async Task SubmitRadicalChacharProcessingErrorTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_radicalChachar4, HttpStatusCode.InternalServerError);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        await SubmitAsync(_radicalChachar4, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(PROCESSING_ERROR);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        Assert.That(_chachars, Does.Not.Contain(_radicalChachar4));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_CHARACTER
        );
    }

    [Test]
    public async Task SubmitNonRadicalChacharWithoutAlternativeProcessingErrorTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithoutAlternative32, HttpStatusCode.InternalServerError);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setRadicalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);

        await SelectRadicalAsync(
            setRadicalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER
        );

        await SubmitAsync(_nonRadicalChacharWithoutAlternative32, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(PROCESSING_ERROR);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_CHARACTER, calledTimes: 3);
        Assert.That(_chachars, Does.Not.Contain(_nonRadicalChacharWithoutAlternative32));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_CHARACTER
        );
    }

    [Test]
    public async Task SubmitNonRadicalChacharWithAlternativeProcessingErrorTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithAlternative12, HttpStatusCode.InternalServerError);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setRadicalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL).SetVoidResult();
        var setAlternativeSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL_ALTERNATIVE).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);

        await SelectRadicalAndAlternative(
            setRadicalSelectorTitleHandler,
            setAlternativeSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_RADICAL,
            SELECT_RADICAL_ALTERNATIVE,
            CREATE_NEW_CHARACTER
        );

        await SubmitAsync(_nonRadicalChacharWithAlternative12, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(PROCESSING_ERROR);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_CHARACTER, calledTimes: 4);
        Assert.That(_chachars, Does.Not.Contain(_nonRadicalChacharWithAlternative12));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL_ALTERNATIVE,
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_CHARACTER
        );
    }

    [Test]
    public async Task SubmitRadicalChacharOkTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_radicalChachar4, HttpStatusCode.OK);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setProcessDialogSuccessTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SUCCESS).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        await SubmitAsync(_radicalChachar4, setProcessDialogSuccessTitleHandler, SUCCESS);

        _entityModalTestCommons.ProcessDialogSuccessTest(
            CHARACTER_CREATED,
            _radicalChachar4.TheCharacter!,
            _radicalChachar4.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setIndexTitleHandler, INDEX_TITLE);
        _entityModalTestCommons.ModalClosedTest(IDs.CHACHAR_FORM_ROOT);
        Assert.That(_chachars, Does.Contain(_radicalChachar4));

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            SUCCESS,
            INDEX_TITLE
        );
    }

    [Test]
    public async Task SubmitNonRadicalChacharWithoutAlternativeOkTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithoutAlternative32, HttpStatusCode.OK);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setRadicalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL).SetVoidResult();
        var setProcessDialogSuccessTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SUCCESS).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);

        await SelectRadicalAsync(
            setRadicalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER
        );

        await SubmitAsync(_nonRadicalChacharWithoutAlternative32, setProcessDialogSuccessTitleHandler, SUCCESS);

        _entityModalTestCommons.ProcessDialogSuccessTest(
            CHARACTER_CREATED,
            _nonRadicalChacharWithoutAlternative32.TheCharacter!,
            _nonRadicalChacharWithoutAlternative32.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_chachars, Does.Contain(_nonRadicalChacharWithoutAlternative32));
        _entityModalTestCommons.ModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            SUCCESS,
            INDEX_TITLE
        );
    }


    [Test]
    public async Task SubmitNonRadicalChacharWithAlternativeOkTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithAlternative12, HttpStatusCode.OK);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setRadicalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL).SetVoidResult();
        var setAlternativeSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL_ALTERNATIVE).SetVoidResult();
        var setProcessDialogSuccessTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SUCCESS).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);

        await SelectRadicalAndAlternative(
            setRadicalSelectorTitleHandler,
            setAlternativeSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_RADICAL,
            SELECT_RADICAL_ALTERNATIVE,
            CREATE_NEW_CHARACTER
        );

        await SubmitAsync(_nonRadicalChacharWithAlternative12, setProcessDialogSuccessTitleHandler, SUCCESS);

        _entityModalTestCommons.ProcessDialogSuccessTest(
            CHARACTER_CREATED,
            _nonRadicalChacharWithAlternative12.TheCharacter!,
            _nonRadicalChacharWithAlternative12.RealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_chachars, Does.Contain(_nonRadicalChacharWithAlternative12));
        _entityModalTestCommons.ModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL,
            CREATE_NEW_CHARACTER,
            SELECT_RADICAL_ALTERNATIVE,
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            SUCCESS,
            INDEX_TITLE
        );
    }

    private async Task SubmitRadicalChacharAlreadyExistsNonKeyFieldDiffersTest(Chachar chachar, Chachar chacharClone)
    {
        await SubmitChacharAlreadyExistsTest(chachar);
        Assert.That(_chachars, Does.Contain(chacharClone));
        // The clone is not in the list, but the presence is decided only by key fields. This is expected state.
    }

    private async Task SubmitChacharAlreadyExistsTest(Chachar chachar)
    {
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        var expectedErrorMessage = string.Format(
            CultureInfo.InvariantCulture,
            CHARACTER_ALREADY_EXISTS,
            chachar.TheCharacter,
            chachar.RealPinyin
        );

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        await SubmitAsync(chachar, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(expectedErrorMessage);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_CHARACTER);
        Assert.That(_chachars, Does.Contain(chachar));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.CHACHAR_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_CHARACTER,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_CHARACTER
        );
    }

    private async Task SelectRadicalAsync(
        JSRuntimeInvocationHandler setTitleHandler,
        JSRuntimeInvocationHandler setAfterClickTitleHandler,
        string expectedTitle,
        string expectedAfterClickTitle
    ) =>
        await _entityFormTestCommons.ClickFirstInSelector(
            IDs.CHACHAR_FORM_RADICAL_INPUT,
            CssClasses.RADICAL_SELECTOR,
            setTitleHandler,
            setAfterClickTitleHandler,
            expectedTitle,
            expectedAfterClickTitle
        );

    private async Task SelectRadicalAndAlternative(
        JSRuntimeInvocationHandler setRadicalSelectorTitleHandler,
        JSRuntimeInvocationHandler setAlternativeSelectorTitleHandler,
        JSRuntimeInvocationHandler setAfterClickTitleHandler,
        string expectedRadicalSelectorTitle,
        string expectedAlternativeSelectorTitle,
        string expectedAfterClickTitle
    )
    {
        await SelectRadicalAsync(
            setRadicalSelectorTitleHandler,
            setAfterClickTitleHandler,
            expectedRadicalSelectorTitle,
            expectedAfterClickTitle
        );

        await _entityFormTestCommons.ClickFirstInSelector(
            IDs.CHACHAR_FORM_ALTERNATIVE_INPUT,
            CssClasses.ALTERNATIVE_SELECTOR,
            setAlternativeSelectorTitleHandler,
            setAfterClickTitleHandler,
            expectedAlternativeSelectorTitle,
            expectedAfterClickTitle,
            afterClickCalledTimes: 3
        );
    }

    private async Task SubmitAsync(Chachar chachar, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_PINYIN_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_IPA_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, chachar.TheCharacter).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_PINYIN_INPUT, chachar.Pinyin).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_IPA_INPUT, chachar.Ipa).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, chachar.Tone).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, chachar.Strokes).SetVoidResult();

        var theCharacterInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_THE_CHARACTER_INPUT}");
        var pinyinInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_PINYIN_INPUT}");
        var ipaInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_IPA_INPUT}");
        var toneInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_TONE_INPUT}");
        var strokesInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_STROKES_INPUT}");
        var formSubmitButton = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_SUBMIT_BUTTON}");

        theCharacterInput.Change(chachar.TheCharacter);
        pinyinInput.Change(chachar.Pinyin);
        ipaInput.Change(chachar.Ipa);
        toneInput.Change(chachar.Tone);
        strokesInput.Change(chachar.Strokes);

        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        await formSubmitButton.ClickAsync(_mouseEventArgs);
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
    }
}
