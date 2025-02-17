using AngleSharp.Diffing.Extensions;
using Asciipinyin.Web.Client.Test.Commons;
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
using AsciiPinyin.Web.Shared.Utils;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System.Globalization;
using System.Net;
using System.Text;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.ChacharsTabComponents;

[TestFixture]
internal sealed class ChacharFormTest : IDisposable
{
    private const string CHARACTER_ALREADY_IN_DB = "Character '{0}' - '{1}' already in DB";
    private const string CHARACTER_CREATED = "Character '{0}' - '{1}' created";
    private const string COMPULSORY_VALUE = nameof(COMPULSORY_VALUE);
    private const string CREATE_NEW_CHARACTER = nameof(CREATE_NEW_CHARACTER);
    private const string MUST_BE_CHINESE_CHARACTER = nameof(MUST_BE_CHINESE_CHARACTER);
    private const string ONLY_ASCII_ALLOWED = nameof(ONLY_ASCII_ALLOWED);
    private const string ONLY_IPA_ALLOWED = nameof(ONLY_IPA_ALLOWED);
    private const string PROCESSING_ERROR = nameof(PROCESSING_ERROR);
    private const string SELECT_RADICAL = nameof(SELECT_RADICAL);
    private const string SELECT_RADICAL_ALTERNATIVE = nameof(SELECT_RADICAL_ALTERNATIVE);

    private static readonly IEnumerable<string> _inputIds =
    [
        IDs.CHACHAR_FORM_IPA_INPUT,
        IDs.CHACHAR_FORM_PINYIN_INPUT,
        IDs.CHACHAR_FORM_STROKES_INPUT,
        IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
        IDs.CHACHAR_FORM_TONE_INPUT,
    ];

    private static readonly Chachar _radicalChachar = new()
    {
        TheCharacter = "雨",
        Pinyin = "yu",
        Ipa = "y:",
        Tone = 3,
        Strokes = 8
    };

    private static readonly Chachar _anotherRadicalChachar = new()
    {
        TheCharacter = "辵",
        Pinyin = "chuo",
        Ipa = "ʈʂʰuɔ",
        Tone = 4,
        Strokes = 7
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative = new()
    {
        TheCharacter = "零",
        Pinyin = "ling",
        Ipa = "liŋ",
        Tone = 2,
        Strokes = 13,
        RadicalCharacter = "雨",
        RadicalPinyin = "yu",
        RadicalTone = 3,
        RadicalAlternativeCharacter = "⻗"
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative = new()
    {
        TheCharacter = "四",
        Pinyin = "si",
        Ipa = "sɹ̩",
        Tone = 4,
        Strokes = 5,
        RadicalCharacter = "儿",
        RadicalPinyin = "er",
        RadicalTone = 2
    };

    private static readonly Alternative _alternative = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = "雨",
        OriginalPinyin = "yu",
        OriginalTone = 3,
        Strokes = 8
    };

    private static readonly Alternative _anotherAlternative = new()
    {
        TheCharacter = "⻌",
        OriginalCharacter = "辵",
        OriginalPinyin = "chuo",
        OriginalTone = 4,
        Strokes = 3
    };

    private static readonly HashSet<Chachar> _radicalChachars = [_radicalChachar, _anotherRadicalChachar];
    private static readonly HashSet<Alternative> _alternatives = [_alternative, _anotherAlternative];
    private static readonly CompositeFormat _characterAlreadyInDb = CompositeFormat.Parse(CHARACTER_ALREADY_IN_DB);
    private static readonly CompositeFormat _characterCreated = CompositeFormat.Parse(CHARACTER_CREATED);
    private static readonly MouseEventArgs _mouseEventArgs = new();

    private readonly Mock<IEntityClient> _entityClientMock = new();
    private readonly Mock<IIndex> _indexMock = new();
    private readonly Mock<ISubmitDialog> _submitDialogMock = new();
    private readonly Mock<IStringLocalizer<Resource>> _localizerMock = new();

    private HashSet<Chachar> _chachars = default!;
    private EntityFormTestCommons _entityFormTestCommons = default!;
    private IRenderedComponent<ChacharForm> _chacharFormComponent = default!;
    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _ = _indexMock
            .Setup(index => index.SubmitDialog)
            .Returns(_submitDialogMock.Object);

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.CharacterAlreadyInDb])
            .Returns(new LocalizedString(CHARACTER_ALREADY_IN_DB, CHARACTER_ALREADY_IN_DB));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.CharacterCreated])
            .Returns(new LocalizedString(CHARACTER_CREATED, CHARACTER_CREATED));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.CompulsoryValue])
            .Returns(new LocalizedString(COMPULSORY_VALUE, COMPULSORY_VALUE));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.CreateNewCharacter])
            .Returns(new LocalizedString(CREATE_NEW_CHARACTER, CREATE_NEW_CHARACTER));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.MustBeChineseCharacter])
            .Returns(new LocalizedString(MUST_BE_CHINESE_CHARACTER, MUST_BE_CHINESE_CHARACTER));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.OnlyAsciiAllowed])
            .Returns(new LocalizedString(ONLY_ASCII_ALLOWED, ONLY_ASCII_ALLOWED));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.OnlyIpaAllowed])
            .Returns(new LocalizedString(ONLY_IPA_ALLOWED, ONLY_IPA_ALLOWED));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.ProcessingError])
            .Returns(new LocalizedString(PROCESSING_ERROR, PROCESSING_ERROR));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.SelectRadical])
            .Returns(new LocalizedString(SELECT_RADICAL, SELECT_RADICAL));

        _ = _localizerMock
            .Setup(localizer => localizer[Resource.SelectRadicalAlternative])
            .Returns(new LocalizedString(SELECT_RADICAL_ALTERNATIVE, SELECT_RADICAL_ALTERNATIVE));
    }

    [SetUp]
    public void SetUp()
    {
        _chachars = [];

        _ = _indexMock
            .Setup(index => index.Alternatives)
            .Returns(_alternatives);

        _ = _indexMock
            .Setup(index => index.Chachars)
            .Returns(_chachars);

        _testContext = new TestContext();

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_BLOCK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_FLEX).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_NONE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.SHOW).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_BLOCK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_FLEX).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_NONE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.SHOW).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.DISABLE, IDs.CHACHAR_FORM_ALTERNATIVE_INPUT).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.DISABLE, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ENABLE, IDs.CHACHAR_FORM_ALTERNATIVE_INPUT).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ENABLE, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_BLOCK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_FLEX).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.D_NONE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_ALTERNATIVE_SELECTOR, CssClasses.SHOW).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_RADICAL_INPUT, CssClasses.BORDER_DANGER).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_BLOCK).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_FLEX).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.D_NONE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.REMOVE_CLASS, IDs.CHACHAR_FORM_RADICAL_SELECTOR, CssClasses.SHOW).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, CREATE_NEW_CHARACTER).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TITLE, SELECT_RADICAL_ALTERNATIVE).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_Z_INDEX, IDs.CHACHAR_FORM_ROOT, 1).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_Z_INDEX, IDs.CHACHAR_FORM_ROOT, 3).SetVoidResult();

        _ = _testContext.Services
            .AddSingleton(_entityClientMock.Object)
            .AddSingleton(_localizerMock.Object)
            .AddSingleton<IEntityFormCommons, EntityFormCommons>()
            .AddSingleton<IJSInteropDOM, JSInteropDOM>()
            .AddSingleton<IModalCommons, ModalCommons>();

        _chacharFormComponent = _testContext.RenderComponent<ChacharForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock.Object));

        _entityFormTestCommons = new(
            _testContext,
            _chacharFormComponent,
            _entityClientMock,
            _submitDialogMock,
            _inputIds
        );
    }

    [TearDown]
    public void TearDown() => _testContext.Dispose();

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
        _entityFormTestCommons.VerifyInputValueSet(inputValue, expectedContent, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT);

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

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - empty string")]
    [TestCase("0", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - zero")]
    [TestCase("1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single digit positive integer")]
    [TestCase(" ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - space")]
    [TestCase("\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - new line")]
    [TestCase("\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - tabluar")]
    [TestCase("\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - backslash")]
    [TestCase("\'", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - apostrophe")]
    [TestCase("\"", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - quotes")]
    [TestCase("`", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - backtick")]
    [TestCase(".", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - dot")]
    [TestCase(":", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - colon")]
    [TestCase(";", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - semicolon")]
    [TestCase("@", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - at sign")]
    [TestCase("#", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - hash sign")]
    [TestCase("$", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - dollar sign")]
    [TestCase("{", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - closing curly bracket")]
    [TestCase("a", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character lowercase")]
    [TestCase("A", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character uppercase")]
    [TestCase("ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character lowercase")]
    [TestCase("Ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character uppercase")]
    [TestCase("ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - deprecated version")]
    public void TheCharacterWrongSubmitTest(string inputValue, string expectedError)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        _entityFormTestCommons.WrongSubmitOnInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_THE_CHARACTER_ERROR
        );
    }

    [TestCase("中", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension G")]
    public void TheCharacterCorrectSubmitTest(string inputValue)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, TextUtils.GetStringFirstCharacterAsString(inputValue));

        _entityFormTestCommons.CorrectSubmitOnInputTest(
            inputValue,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_THE_CHARACTER_ERROR
        );
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - empty string")]
    [TestCase("0", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - zero")]
    [TestCase("1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single digit positive integer")]
    [TestCase("-1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single digit negative integer")]
    [TestCase("123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits positive integer")]
    [TestCase("-123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits negative integer")]
    [TestCase("0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits positive float")]
    [TestCase("-0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits negative float")]
    [TestCase("123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits positive float")]
    [TestCase("-123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits negative float")]
    [TestCase(" ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - space")]
    [TestCase("   ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple spaces")]
    [TestCase("\n", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - new line")]
    [TestCase("\n\n\n", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple new lines")]
    [TestCase("\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - tabluar")]
    [TestCase("\t\t\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple tabulars")]
    [TestCase(" \n\t", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - space, new line and tabular together")]
    [TestCase("\\", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - backslash")]
    [TestCase("\'", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - apostrophe")]
    [TestCase("\"", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - quotes")]
    [TestCase("`", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - backtick")]
    [TestCase(".", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - dot")]
    [TestCase(":", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - colon")]
    [TestCase(";", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - semicolon")]
    [TestCase("@", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - at sign")]
    [TestCase("#", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - hash sign")]
    [TestCase("$", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - dollar sign")]
    [TestCase("{", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - closing curly bracket")]
    [TestCase("{0}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single pinyin character lowercase")]
    [TestCase("Ā", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void PinyinWrongSubmitTest(string inputValue, string expectedError) =>
        _entityFormTestCommons.WrongSubmitOnChangeTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_PINYIN_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_PINYIN_ERROR
        );

    [TestCase("a", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - single ASCII character lowercase")]
    [TestCase("A", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - single ASCII character uppercase")]
    [TestCase("abc", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters case combination")]
    public void PinyinCorrectSubmitTest(string inputValue) =>
        _entityFormTestCommons.CorrectSubmitOnChangeTest(
            inputValue,
            IDs.CHACHAR_FORM_PINYIN_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_PINYIN_ERROR
        );

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - empty string")]
    [TestCase("0", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - zero")]
    [TestCase("1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single digit positive integer")]
    [TestCase("-1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single digit negative integer")]
    [TestCase("123", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple digits positive integer")]
    [TestCase("-123", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple digits negative integer")]
    [TestCase("0.1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - two digits positive float")]
    [TestCase("-0.1", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - two digits negative float")]
    [TestCase("123.456", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple digits positive float")]
    [TestCase("-123.456", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple digits negative float")]
    [TestCase(" ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - space")]
    [TestCase("   ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple spaces")]
    [TestCase("\n", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - new line")]
    [TestCase("\n\n\n", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple new lines")]
    [TestCase("\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - tabluar")]
    [TestCase("\t\t\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple tabulars")]
    [TestCase(" \n\t", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - space, new line and tabular together")]
    [TestCase("\\", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - backslash")]
    [TestCase("\"", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - quotes")]
    [TestCase("`", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - backtick")]
    [TestCase(";", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - semicolon")]
    [TestCase("@", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - at sign")]
    [TestCase("#", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - hash sign")]
    [TestCase("$", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - dollar sign")]
    [TestCase("{", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - closing curly bracket")]
    [TestCase("{0}", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("A", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single ASCII character uppercase")]
    [TestCase("ABC", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single pinyin character lowercase")]
    [TestCase("Ā", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("ɼ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", ONLY_IPA_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaWrongSubmitTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void IpaWrongSubmitTest(string inputValue, string expectedError) =>
        _entityFormTestCommons.WrongSubmitOnChangeTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_IPA_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_IPA_ERROR
        );

    [TestCase("\'", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - apostrophe")]
    [TestCase(".", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - dot")]
    [TestCase(":", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - colon")]
    [TestCase("a", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - single ASCII character lowercase")] // Lowercase ASCII characters are valid IPA
    [TestCase("abc", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - multiple ASCII characters lowercase")]
    [TestCase("r̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("r̝r̻̝r̝̊", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", TestName = $"{nameof(ChacharFormTest)}.{nameof(IpaCorrectSubmitTest)} - Czech text in IPA")]
    public void IpaCorrectSubmitTest(string inputValue) =>
        _entityFormTestCommons.CorrectSubmitOnChangeTest(
            inputValue,
            IDs.CHACHAR_FORM_IPA_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_IPA_ERROR
        );

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value one")]
    [TestCase("4", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputAdjustedTest)} - previous value four")]
    public void ToneOnInputAdjustedTest(string previousValidInputValue) =>
        _entityFormTestCommons.NumberInputAdjustedTest(previousValidInputValue, IDs.CHACHAR_FORM_TONE_INPUT);

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - one")]
    [TestCase("4", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneOnInputUnchangedTest)} - four")]
    public void ToneOnInputUnchangedTest(string inputValue) =>
        _entityFormTestCommons.NumberInputUnchangedTest(inputValue, IDs.CHACHAR_FORM_TONE_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneWrongSubmitTest)} - empty string")]
    public void ToneWrongSubmitTest(string inputValue, string expectedError)
    {
        // Empty tone is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, inputValue);

        _entityFormTestCommons.WrongSubmitOnInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_TONE_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_TONE_ERROR
        );
    }

    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneCorrectSubmitTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneCorrectSubmitTest)} - one")]
    [TestCase("4", TestName = $"{nameof(ChacharFormTest)}.{nameof(ToneCorrectSubmitTest)} - four")]
    public void ToneCorrectSubmitTest(string inputValue)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, inputValue);

        _entityFormTestCommons.CorrectSubmitOnInputTest(
            inputValue,
            IDs.CHACHAR_FORM_TONE_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_TONE_ERROR
        );
    }

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value one")]
    [TestCase("9", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value nine")]
    [TestCase("13", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value thirteen")]
    [TestCase("66", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value sixty-six")]
    [TestCase("99", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value ninety-nine")]
    public void StrokesOnInputAdjustedTest(string previousValidInputValue) =>
        _entityFormTestCommons.NumberInputAdjustedTest(previousValidInputValue, IDs.CHACHAR_FORM_STROKES_INPUT);

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - one")]
    [TestCase("9", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - ninety-nine")]
    public void StrokesOnInputUnchangedTest(string inputValue) =>
        _entityFormTestCommons.NumberInputUnchangedTest(inputValue, IDs.CHACHAR_FORM_STROKES_INPUT);

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesWrongSubmitTest)} - empty string")]
    public void StrokesWrongSubmitTest(string inputValue, string expectedError)
    {
        // Empty strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventStrokesInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, inputValue);

        _entityFormTestCommons.WrongSubmitOnInputTest(
            inputValue,
            expectedError,
            IDs.CHACHAR_FORM_STROKES_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_STROKES_ERROR
        );
    }

    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - one")]
    [TestCase("9", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(ChacharFormTest)}.{nameof(StrokesCorrectSubmitTest)} - ninety-nine")]
    public void StrokesCorrectSubmitTest(string inputValue)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, inputValue);

        _entityFormTestCommons.CorrectSubmitOnInputTest(
            inputValue,
            IDs.CHACHAR_FORM_STROKES_INPUT,
            IDs.CHACHAR_FORM_SUBMIT_BUTTON,
            IDs.CHACHAR_FORM_STROKES_ERROR
        );
    }

    [Test]
    public async Task RadicalChacharAlreadyExistsSubmitTest()
    {
        _chachars.AddRange(
            _radicalChachars
                .Concat([_nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative])
        );

        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SubmitAsync(_radicalChachar);

        CharacterAlreadyExistsSubmitDialogErrorMessageTest(errorMessage, _radicalChachar);
    }

    [Test]
    public async Task NonRadicalChacharWithoutAlternativeAlreadyExistsSubmitTest()
    {
        _chachars.AddRange(
            _radicalChachars
                .Concat([_nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative])
        );

        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SelectRadicalAsync();
        await SubmitAsync(_nonRadicalChacharWithoutAlternative);

        CharacterAlreadyExistsSubmitDialogErrorMessageTest(errorMessage, _nonRadicalChacharWithoutAlternative);
    }

    [Test]
    public async Task NonRadicalChacharWithAlternativeAlreadyExistsSubmitTest()
    {
        _chachars.AddRange(
            _radicalChachars
                .Concat([_nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative])
        );

        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SelectRadicalAndAlternativeAsync();
        await SubmitAsync(_nonRadicalChacharWithAlternative);

        CharacterAlreadyExistsSubmitDialogErrorMessageTest(errorMessage, _nonRadicalChacharWithAlternative);
    }

    [Test]
    public async Task RadicalChacharSubmitProcessingErrorTest()
    {
        _chachars.AddRange([_nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative]);
        _entityFormTestCommons.MockPostStatusCode(_radicalChachar, HttpStatusCode.InternalServerError);
        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SubmitAsync(_radicalChachar);

        _entityFormTestCommons.SubmitDialogErrorMessageTest(errorMessage, PROCESSING_ERROR);
    }

    [Test]
    public async Task NonRadicalChacharWithoutAlternativeSubmitProcessingErrorTest()
    {
        _chachars.AddRange(_radicalChachars);
        _ = _chachars.Add(_nonRadicalChacharWithAlternative);
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithoutAlternative, HttpStatusCode.InternalServerError);
        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SelectRadicalAsync();
        await SubmitAsync(_nonRadicalChacharWithoutAlternative);

        _entityFormTestCommons.SubmitDialogErrorMessageTest(errorMessage, PROCESSING_ERROR);
    }

    [Test]
    public async Task NonRadicalChacharWithAlternativeSubmitProcessingErrorTest()
    {
        _chachars.AddRange(_radicalChachars);
        _ = _chachars.Add(_nonRadicalChacharWithoutAlternative);
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithAlternative, HttpStatusCode.InternalServerError);
        string? errorMessage = null;
        _entityFormTestCommons.CaptureErrorMessage(actualErrorMessage => errorMessage = actualErrorMessage);
        await SelectRadicalAndAlternativeAsync();
        await SubmitAsync(_nonRadicalChacharWithAlternative);

        _entityFormTestCommons.SubmitDialogErrorMessageTest(errorMessage, PROCESSING_ERROR);
    }

    [Test]
    public async Task RadicalChacharSubmitOkTest()
    {
        _chachars.AddRange([_nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative]);
        _entityFormTestCommons.MockPostStatusCode(_radicalChachar, HttpStatusCode.OK);
        string? successMessage = null;
        _entityFormTestCommons.CaptureSuccessMessage(actualSuccessMessage => successMessage = actualSuccessMessage);
        await SubmitAsync(_radicalChachar);

        CharacterCreatedSubmitDialogSuccessMessageTest(successMessage, _radicalChachar);
    }

    [Test]
    public async Task NonRadicalChacharWithoutAlternativeSubmitOkTest()
    {
        _chachars.AddRange(_radicalChachars);
        _ = _chachars.Add(_nonRadicalChacharWithAlternative);
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithoutAlternative, HttpStatusCode.OK);
        string? successMessage = null;
        _entityFormTestCommons.CaptureSuccessMessage(actualSuccessMessage => successMessage = actualSuccessMessage);
        await SelectRadicalAsync();
        await SubmitAsync(_nonRadicalChacharWithoutAlternative);

        CharacterCreatedSubmitDialogSuccessMessageTest(successMessage, _nonRadicalChacharWithoutAlternative);
    }

    [Test]
    public async Task NonRadicalChacharWithAlternativeSubmitOkTest()
    {
        _chachars.AddRange(_radicalChachars);
        _ = _chachars.Add(_nonRadicalChacharWithoutAlternative);
        _entityFormTestCommons.MockPostStatusCode(_nonRadicalChacharWithAlternative, HttpStatusCode.OK);
        string? successMessage = null;
        _entityFormTestCommons.CaptureSuccessMessage(actualSuccessMessage => successMessage = actualSuccessMessage);
        await SelectRadicalAndAlternativeAsync();
        await SubmitAsync(_nonRadicalChacharWithAlternative);

        CharacterCreatedSubmitDialogSuccessMessageTest(successMessage, _nonRadicalChacharWithAlternative);
    }

    private async Task SelectRadicalAsync()
    {
        //Open radical selector
        var openRadicalSelectorButton = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_RADICAL_INPUT}");
        Assert.That(openRadicalSelectorButton, Is.Not.Null);
        await openRadicalSelectorButton.ClickAsync(_mouseEventArgs);

        // Click on the first button in the selector
        var buttonDivs = _chacharFormComponent.FindAll("div").Where(div => div.ClassList.Contains(CssClasses.RADICAL_SELECTOR));
        Assert.That(buttonDivs.Count(), Is.EqualTo(_radicalChachars.Count));
        var radicalButtonDiv = buttonDivs.First();
        await radicalButtonDiv.ClickAsync(_mouseEventArgs);
    }

    private async Task SelectRadicalAndAlternativeAsync()
    {
        await SelectRadicalAsync();

        // Open alternative selector
        var openAlternativeSelectorButton = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_ALTERNATIVE_INPUT}");
        Assert.That(openAlternativeSelectorButton, Is.Not.Null);
        await openAlternativeSelectorButton.ClickAsync(_mouseEventArgs);

        // Click on the first button in the selector
        // Alternatives must be mocked so that the first one in the list is the desired one
        var buttonDivs = _chacharFormComponent.FindAll("div").Where(div => div.ClassList.Contains(CssClasses.ALTERNATIVE_SELECTOR));
        var alternativeButtonDiv = buttonDivs.First();
        await alternativeButtonDiv.ClickAsync(_mouseEventArgs);
    }

    private async Task SubmitAsync(Chachar chachar)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_PINYIN_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_IPA_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_TONE_INPUT).SetResult(true);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.CHACHAR_FORM_STROKES_INPUT).SetResult(true);

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, chachar.TheCharacter).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_PINYIN_INPUT, chachar.Pinyin).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_IPA_INPUT, chachar.Ipa).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_TONE_INPUT, chachar.Tone.ToString()).SetVoidResult();
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_STROKES_INPUT, chachar.Strokes.ToString()).SetVoidResult();

        var theCharacterInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_THE_CHARACTER_INPUT}");
        var pinyinInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_PINYIN_INPUT}");
        var ipaInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_IPA_INPUT}");
        var toneInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_TONE_INPUT}");
        var strokesInput = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_STROKES_INPUT}");
        var formSubmitButton = _chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_SUBMIT_BUTTON}");

        // Inputs with 'oninput' attribute => Input
        // Inputs with 'bind-value' attribute => Change
        theCharacterInput.Input(chachar.TheCharacter);
        pinyinInput.Change(chachar.Pinyin);
        ipaInput.Change(chachar.Ipa);
        toneInput.Input(chachar.Tone);
        strokesInput.Input(chachar.Strokes);

        await formSubmitButton.ClickAsync(_mouseEventArgs);
    }

    private void CharacterAlreadyExistsSubmitDialogErrorMessageTest(string? errorMessage, Chachar chachar)
    {
        var expectedErrorMessage = string.Format(
            CultureInfo.InvariantCulture,
            _characterAlreadyInDb,
            chachar.TheCharacter,
            chachar.RealPinyin
        );

        _entityFormTestCommons.SubmitDialogErrorMessageTest(errorMessage, expectedErrorMessage);
    }

    private void CharacterCreatedSubmitDialogSuccessMessageTest(string? successMessage, Chachar chachar)
    {
        var expectedSuccessMessage = string.Format(
            CultureInfo.InvariantCulture,
            _characterCreated,
            chachar.TheCharacter,
            chachar.RealPinyin
        );

        _entityFormTestCommons.SubmitDialogSuccessMessageTest(successMessage, expectedSuccessMessage);
    }

    public void Dispose() => TearDown();
}
