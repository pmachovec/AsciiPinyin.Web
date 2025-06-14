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

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.AlternativesTabComponents;

internal sealed class AlternativeFormTest : IDisposable
{
    private const string ALTERNATIVE_ALREADY_EXISTS = "Alternative {0} for character {1} ({2}) already in exists";
    private const string ALTERNATIVE_CREATED = "Alternative {0} for character {1} ({2}) created";

    private const string COMPULSORY_VALUE = nameof(COMPULSORY_VALUE);
    private const string CREATE_NEW_ALTERNATIVE = nameof(CREATE_NEW_ALTERNATIVE);
    private const string ERROR = nameof(ERROR);
    private const string INDEX_TITLE = nameof(INDEX_TITLE);
    private const string MUST_BE_CHINESE_CHARACTER = nameof(MUST_BE_CHINESE_CHARACTER);
    private const string PROCESSING = nameof(PROCESSING);
    private const string PROCESSING_DOTS = $"{PROCESSING}...";
    private const string PROCESSING_ERROR = nameof(PROCESSING_ERROR);
    private const string SELECT_BASE_CHARACTER = nameof(SELECT_BASE_CHARACTER);
    private const string SUCCESS = nameof(SUCCESS);

    private static readonly Chachar _radicalChachar1 = new()
    {
        TheCharacter = "丿",
        Pinyin = "pie",
        Ipa = "pʰie",
        Tone = 3,
        Strokes = 1
    };

    private static readonly Alternative _alternative11 = new()
    {
        TheCharacter = "乁",
        OriginalCharacter = _radicalChachar1.TheCharacter,
        OriginalPinyin = _radicalChachar1.Pinyin,
        OriginalTone = _radicalChachar1.Tone,
        Strokes = 1
    };

    private static readonly Alternative _alternative12 = new()
    {
        TheCharacter = "乀",
        OriginalCharacter = _radicalChachar1.TheCharacter,
        OriginalPinyin = _radicalChachar1.Pinyin,
        OriginalTone = _radicalChachar1.Tone,
        Strokes = 1
    };

    private static readonly HashSet<Chachar> _chachars = [_radicalChachar1];
    private static readonly MouseEventArgs _mouseEventArgs = new();

    private static readonly Mock<IEntityClient> _entityClientMock = new();
    private static readonly Mock<IIndex> _indexMock = new();
    private static readonly Mock<IStringLocalizer<Resource>> _localizerMock = new();

    private static readonly LocalizerMockSetter _localizerMockSetter = new(_localizerMock);

    private HashSet<Alternative> _alternatives = default!;
    private EntityFormTestCommons<Alternative> _entityFormTestCommons = default!;
    private EntityModalTestCommons<Alternative> _entityModalTestCommons = default!;
    private IRenderedComponent<AlternativeForm> _alternativeFormComponent = default!;
    private IRenderedComponent<Backdrop> _backdropComponent = default!;
    private IRenderedComponent<ProcessDialog> _processDialogComponent = default!;
    private JSInteropSetter _jsInteropSetter = default!;
    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _localizerMockSetter.SetUpResources(
            (Resource.AlternativeAlreadyExists, ALTERNATIVE_ALREADY_EXISTS),
            (Resource.AlternativeCreated, ALTERNATIVE_CREATED),
            (Resource.CreateNewAlternative, CREATE_NEW_ALTERNATIVE),
            (Resource.CompulsoryValue, COMPULSORY_VALUE),
            (Resource.Error, ERROR),
            (Resource.MustBeChineseCharacter, MUST_BE_CHINESE_CHARACTER),
            (Resource.Processing, PROCESSING),
            (Resource.ProcessingError, PROCESSING_ERROR),
            (Resource.SelectBaseCharacter, SELECT_BASE_CHARACTER),
            (Resource.Success, SUCCESS)
        );
    }

    [SetUp]
    public void SetUp()
    {
        _alternatives = [_alternative11];

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
            PROCESSING_DOTS,
            SELECT_BASE_CHARACTER
        );

        _jsInteropSetter.SetUpSetZIndex(IDs.ALTERNATIVE_FORM_ROOT);

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

        _alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(
            parameters => parameters.Add(parameter => parameter.Index, _indexMock.Object)
        );

        _entityFormTestCommons = new(
            _alternativeFormComponent,
            _backdropComponent,
            _testContext.JSInterop,
            _entityClientMock,
            _indexMock,
            IDs.ALTERNATIVE_FORM_ROOT,
            IDs.INDEX_BACKDROP_ROOT
        );

        _entityModalTestCommons = new(
            _alternativeFormComponent,
            _backdropComponent,
            _processDialogComponent,
            _testContext.JSInterop,
            _indexMock,
            IDs.ALTERNATIVE_FORM_ROOT,
            IDs.INDEX_BACKDROP_ROOT
        );
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _testContext.Dispose();

    [TestCase("-1", "-", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - single digit negative integer")]
    [TestCase("123", "1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits positive integer")]
    [TestCase("-123", "-", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits negative integer")]
    [TestCase("0.1", "0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits positive float")]
    [TestCase("-0.1", "-", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits negative float")]
    [TestCase("123.456", "1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits positive float")]
    [TestCase("-123.456", "-", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits negative float")]
    [TestCase("   ", " ", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple spaces")]
    [TestCase("\n\n\n", "\n", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple new lines")]
    [TestCase("\t\t\t", "\t", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple tabulars")]
    [TestCase(" \n\t", " ", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - space, new line and tabular together")]
    [TestCase("{0}", "{", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", "$", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", "a", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", "A", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", "A", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", "T", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", "T", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", "T", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", "T", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", "z", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", "Z", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", "Z", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", "d", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", "d", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", "D", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", "ž", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", "Ž", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", "Ž", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", "P", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", "P", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", "P", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", "P", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", "д", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", "Д", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", "Д", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", "С", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", "С", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", "С", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", "С", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", "r̝", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", "ʈ", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", "p", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", "p", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", "p", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", "p", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", "p", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", "大", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", "大", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", "大", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", "大", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", "大", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", "𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", "𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", "𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", "𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", "𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", "0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void TheCharacterOnInputAdjustedTest(string inputValue, string expectedContent) =>
        _entityFormTestCommons.InputValueSetTest(inputValue, expectedContent, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT);

    [TestCase("", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single digit positive integer")]
    [TestCase(" ", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - space")]
    [TestCase("\n", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - new line")]
    [TestCase("\t", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - tabluar")]
    [TestCase("\\", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - backslash")]
    [TestCase("\'", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - apostrophe")]
    [TestCase("\"", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - quotes")]
    [TestCase("`", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - backtick")]
    [TestCase(".", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - dot")]
    [TestCase(":", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - colon")]
    [TestCase(";", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - semicolon")]
    [TestCase("@", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - at sign")]
    [TestCase("#", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - hash sign")]
    [TestCase("$", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - dollar sign")]
    [TestCase("{", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - opening curly bracket")]
    [TestCase("}", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - closing curly bracket")]
    [TestCase("a", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single ASCII character lowercase")]
    [TestCase("A", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single ASCII character uppercase")]
    [TestCase("ā", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single pinyin character lowercase")]
    [TestCase("Ā", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single pinyin character uppercase")]
    [TestCase("ř", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("中", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single Chinese character - CJK extension G")]
    public void TheCharacterOnInputUnchangedTest(string inputValue) =>
        _entityFormTestCommons.StringInputUnchangedTest(inputValue, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - empty string")]
    [TestCase("0", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - zero")]
    [TestCase("1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - space")]
    [TestCase("\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - new line")]
    [TestCase("\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - quotes")]
    [TestCase("`", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - backtick")]
    [TestCase(".", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - dot")]
    [TestCase(":", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - colon")]
    [TestCase(";", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - at sign")]
    [TestCase("#", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task SubmitTheCharacterWrongTest(string inputValue, string expectedError)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase("中", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitTheCharacterCorrectTest)} - single Chinese character - CJK extension G")]
    public async Task SubmitTheCharacterCorrectTest(string inputValue)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_VALUE,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            TextUtils.GetStringFirstCharacterAsString(inputValue)
         ).SetVoidResult();

        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase(null, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value empty string")]
    [TestCase(0, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value zero")]
    [TestCase(1, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value one")]
    [TestCase(9, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value nine")]
    [TestCase(13, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value thirteen")]
    [TestCase(66, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value sixty-six")]
    [TestCase(99, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value ninety-nine")]
    public void StrokesOnInputAdjustedTest(short? previousValidInput) =>
        _entityFormTestCommons.NumberInputAdjustedTest(previousValidInput, IDs.ALTERNATIVE_FORM_STROKES_INPUT);

    [TestCase(null, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - empty string")]
    [TestCase(0, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - zero")]
    [TestCase(1, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - one")]
    [TestCase(9, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - nine")]
    [TestCase(13, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - thirteen")]
    [TestCase(66, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - sixty-six")]
    [TestCase(99, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - ninety-nine")]
    public void StrokesOnInputUnchangedTest(short? inputValue) =>
        _entityFormTestCommons.NumberInputUnchangedTest(inputValue, IDs.ALTERNATIVE_FORM_STROKES_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesWrongTest)} - empty string")]
    public async Task SubmitStrokesWrongTest(string inputValue, string expectedError)
    {
        // Empty strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventStrokesInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_STROKES_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitInvalidInputTest(
            inputValue,
            expectedError,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            IDs.ALTERNATIVE_FORM_STROKES_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );
    }

    [TestCase("0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - one")]
    [TestCase("9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(AlternativeFormTest)}.{nameof(SubmitStrokesCorrectTest)} - ninety-nine")]
    public async Task SubmitStrokesCorrectTest(string inputValue)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_STROKES_INPUT, inputValue).SetVoidResult();

        await _entityFormTestCommons.SubmitValidInputTest(
            inputValue,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            IDs.ALTERNATIVE_FORM_STROKES_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );
    }

    [Test]
    public async Task SubmitOriginalInvalidTest() =>
        await _entityFormTestCommons.SubmitInvalidButtonInputTest(
            COMPULSORY_VALUE,
            IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT,
            IDs.ALTERNATIVE_FORM_ORIGINAL_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );

    [Test]
    public async Task SubmitOriginalValidTest()
    {
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_ALTERNATIVE).SetVoidResult();
        var setOriginalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_BASE_CHARACTER).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE);

        await SelectOriginalAsync(
            setOriginalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE
        );

        await _entityFormTestCommons.SubmitValidButtonInputTest(
            IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT,
            IDs.ALTERNATIVE_FORM_ORIGINAL_VALIDATION_MESSAGE,
            IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON
        );
    }

    [Test]
    public async Task OpenCloseTest()
    {
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_ALTERNATIVE).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE);
        await _entityModalTestCommons.CloseTest(setIndexTitleHandler, INDEX_TITLE);
        _entityModalTestCommons.TitlesOrderTest(CREATE_NEW_ALTERNATIVE, INDEX_TITLE);
    }

    [Test]
    public async Task SubmitAlternativeAlreadyExistsTest() =>
        await SubmitAlternativeAlreadyExistsTest(_alternative11);

    [Test]
    public async Task SubmitAlternativeAlreadyExistsStrokesDifferTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative11.TheCharacter,
            OriginalCharacter = _alternative11.OriginalCharacter,
            OriginalPinyin = _alternative11.OriginalPinyin,
            OriginalTone = _alternative11.OriginalTone,
            Strokes = (short)(_alternative11.Strokes! + 1)
        };

        await SubmitAlternativeAlreadyExistsTest(alternativeClone);
        Assert.That(_alternatives, Does.Contain(alternativeClone));
        // The clone is not in the list, but the presence is decided only by key fields. This is expected state.
    }

    [Test]
    public async Task SubmitAlternativeProcessingErrorTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_alternative12, HttpStatusCode.InternalServerError);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_ALTERNATIVE).SetVoidResult();
        var setOriginalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_BASE_CHARACTER).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE);

        await SelectOriginalAsync(
            setOriginalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE
        );

        await SubmitAsync(_alternative12, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(PROCESSING_ERROR);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE, calledTimes: 3);
        Assert.That(_alternatives, Does.Not.Contain(_alternative12));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.ALTERNATIVE_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_ALTERNATIVE,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_ALTERNATIVE
        );
    }

    [Test]
    public async Task SubmitAlternativeOkTest()
    {
        _entityFormTestCommons.MockPostStatusCode(_alternative12, HttpStatusCode.OK);
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_ALTERNATIVE).SetVoidResult();
        var setOriginalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_BASE_CHARACTER).SetVoidResult();
        var setProcessDialogSuccessTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SUCCESS).SetVoidResult();
        var setIndexTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, INDEX_TITLE).SetVoidResult();

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE);

        await SelectOriginalAsync(
            setOriginalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE
        );

        await SubmitAsync(_alternative12, setProcessDialogSuccessTitleHandler, SUCCESS);

        _entityModalTestCommons.ProcessDialogSuccessTest(
            ALTERNATIVE_CREATED,
            _alternative12.TheCharacter!,
            _alternative12.OriginalCharacter!,
            _alternative12.OriginalRealPinyin!
        );

        await _entityModalTestCommons.ClickProcessDialogProceedButtonTest(setIndexTitleHandler, INDEX_TITLE);
        Assert.That(_alternatives, Does.Contain(_alternative12));
        _entityModalTestCommons.ModalClosedTest(IDs.ALTERNATIVE_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_ALTERNATIVE,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE,
            PROCESSING_DOTS,
            SUCCESS,
            INDEX_TITLE
        );
    }

    private async Task SubmitAlternativeAlreadyExistsTest(Alternative alternative)
    {
        var setFormTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_ALTERNATIVE).SetVoidResult();
        var setOriginalSelectorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_BASE_CHARACTER).SetVoidResult();
        var setProcessDialogErrorTitleHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, ERROR).SetVoidResult();

        var expectedErrorMessage = string.Format(
            CultureInfo.InvariantCulture,
            ALTERNATIVE_ALREADY_EXISTS,
            alternative.TheCharacter,
            alternative.OriginalCharacter,
            alternative.OriginalRealPinyin
        );

        await _entityFormTestCommons.OpenTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE);

        await SelectOriginalAsync(
            setOriginalSelectorTitleHandler,
            setFormTitleHandler,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE
        );

        await SubmitAsync(alternative, setProcessDialogErrorTitleHandler, ERROR);
        _entityModalTestCommons.ProcessDialogErrorTest(expectedErrorMessage);
        await _entityModalTestCommons.ClickProcessDialogBackButtonTest(setFormTitleHandler, CREATE_NEW_ALTERNATIVE, calledTimes: 3);
        Assert.That(_alternatives, Does.Contain(alternative));
        _entityModalTestCommons.ProcessDialogOverModalClosedTest(IDs.ALTERNATIVE_FORM_ROOT);

        _entityModalTestCommons.TitlesOrderTest(
            CREATE_NEW_ALTERNATIVE,
            SELECT_BASE_CHARACTER,
            CREATE_NEW_ALTERNATIVE,
            PROCESSING_DOTS,
            ERROR,
            CREATE_NEW_ALTERNATIVE
        );
    }

    private async Task SelectOriginalAsync(
        JSRuntimeInvocationHandler setTitleHandler,
        JSRuntimeInvocationHandler setAfterClickTitleHandler,
        string expectedTitle,
        string expectedAfterClickTitle,
        int afterClickCalledTimes = 2
    ) =>
        await _entityFormTestCommons.ClickFirstInSelector(
            IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT,
            CssClasses.ORIGINAL_SELECTOR,
            setTitleHandler,
            setAfterClickTitleHandler,
            expectedTitle,
            expectedAfterClickTitle,
            afterClickCalledTimes
        );

    private async Task SubmitAsync(Alternative alternative, JSRuntimeInvocationHandler setTitleHandler, string expectedTitle)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_VALUE,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            alternative.TheCharacter
        ).SetVoidResult();

        _ = _testContext.JSInterop.SetupVoid(
            DOMFunctions.SET_VALUE,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            alternative.Strokes?.ToString(CultureInfo.InvariantCulture)
        ).SetVoidResult();

        var theCharacterInput = _alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT}");
        var strokesInput = _alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_STROKES_INPUT}");
        var formSubmitButton = _alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON}");

        theCharacterInput.Change(alternative.TheCharacter);
        strokesInput.Change(alternative.Strokes);

        setTitleHandler.VerifyNotInvoke(DOMFunctions.SET_TITLE, expectedTitle);
        await formSubmitButton.ClickAsync(_mouseEventArgs);
        _ = setTitleHandler.VerifyInvoke(DOMFunctions.SET_TITLE, expectedTitle);
    }
}
