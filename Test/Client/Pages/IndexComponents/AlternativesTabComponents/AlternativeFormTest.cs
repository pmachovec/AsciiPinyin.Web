using AngleSharp.Dom;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Constants.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using Index = AsciiPinyin.Web.Client.Pages.Index;
using TestContext = Bunit.TestContext;

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.AlternativesTabComponents;

internal sealed class AlternativeFormTest : IDisposable
{
    private const string COMPULSORY_VALUE = nameof(COMPULSORY_VALUE);
    private const string MUST_BE_CHINESE_CHARACTER = nameof(MUST_BE_CHINESE_CHARACTER);

    private readonly IEnumerable<string> _inputIds =
    [
        IDs.ALTERNATIVE_FORM_STROKES_INPUT,
        IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT
    ];

    private readonly Index _indexMock = Mock.Of<Index>();
    private readonly IStringLocalizer<Resource> _localizerMock = Mock.Of<IStringLocalizer<Resource>>();

    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _ = Mock.Get(_localizerMock).Setup(localizer => localizer[Resource.CompulsoryValue]).Returns(new LocalizedString(COMPULSORY_VALUE, COMPULSORY_VALUE));
        _ = Mock.Get(_localizerMock).Setup(localizer => localizer[Resource.MustBeChineseCharacter]).Returns(new LocalizedString(MUST_BE_CHINESE_CHARACTER, MUST_BE_CHINESE_CHARACTER));
    }

    [SetUp]
    public void SetUp()
    {
        var originalSelectorMock = Mock.Of<EntitySelector<Chachar>>();

        _testContext = new TestContext();
        _ = _testContext.ComponentFactories.Add(originalSelectorMock);
        _ = _testContext.Services.AddSingleton(_localizerMock);
        _ = _testContext.Services.AddSingleton<IEntityFormCommons, EntityFormCommons>();
        _ = _testContext.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();
        _ = _testContext.Services.AddSingleton<IModalWithBackdropCommons, ModalWithBackdropCommons>();
    }

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
    public void TheCharacterOnInputAdjustedTest(string theInput, string expectedContent)
    {
        var alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        VerifyInputValueSet(alternativeFormComponent, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, theInput, expectedContent);
    }

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
    public void TheCharacterOnInputUnchangedTest(string theInput)
    {
        var alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, theInput);
        var alternativeFormTheCharacterInput = alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT}");
        alternativeFormTheCharacterInput.Input(theInput);

        setValueInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_VALUE);
        _ = setValueInvocationHandler.SetVoidResult();
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - empty string")]
    [TestCase("0", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - zero")]
    [TestCase("1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single digit positive integer")]
    [TestCase(" ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - space")]
    [TestCase("\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - new line")]
    [TestCase("\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - tabluar")]
    [TestCase("\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - backslash")]
    [TestCase("\'", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - apostrophe")]
    [TestCase("\"", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - quotes")]
    [TestCase("`", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - backtick")]
    [TestCase(".", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - dot")]
    [TestCase(":", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - colon")]
    [TestCase(";", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - semicolon")]
    [TestCase("@", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - at sign")]
    [TestCase("#", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - hash sign")]
    [TestCase("$", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - dollar sign")]
    [TestCase("{", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - closing curly bracket")]
    [TestCase("a", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character lowercase")]
    [TestCase("A", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character uppercase")]
    [TestCase("ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character lowercase")]
    [TestCase("Ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character uppercase")]
    [TestCase("ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - Czech 'Ř' in IPA - deprecated version")]
    public void TheCharacterWrongSubmitTest(string theInput, string expectedError)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        WrongSubmitTest(
            theInput,
            expectedError,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR);
    }

    [TestCase("中", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - single Chinese character - CJK extension G")]
    public void TheCharacterCorrectSubmitTest(string theInput)
    {
        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to test this case.

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, TextUtils.GetStringFirstCharacterAsString(theInput));
        CorrectSubmitTest(
            theInput,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR);
    }

    [TestCase("", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value empty string")]
    [TestCase("0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value zero")]
    [TestCase("1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value one")]
    [TestCase("9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value nine")]
    [TestCase("13", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value thirteen")]
    [TestCase("66", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value sixty-six")]
    [TestCase("99", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputAdjustedTest)} - previous value ninety-nine")]
    public void StrokesOnInputAdjustedTest(string previousValidInput)
    {
        // Mock the input to be valid first.
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_STROKES_INPUT, previousValidInput);
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        var alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var alternativeFormNumberInput = alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_STROKES_INPUT}");
        alternativeFormNumberInput.Input(previousValidInput);

        // Now mock invalid input and verify that it was changed to the previous valid one.
        // Substitutes all invalid inputs, no need to run the test for each one separately.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(false);
        VerifyInputValueSet(alternativeFormComponent, IDs.ALTERNATIVE_FORM_STROKES_INPUT, It.IsAny<string>(), previousValidInput);
    }

    [TestCase("", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - one")]
    [TestCase("9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesOnInputUnchangedTest)} - ninety-nine")]
    public void StrokesOnInputUnchangedTest(string theInput)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        var alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        VerifyInputValueSet(alternativeFormComponent, IDs.ALTERNATIVE_FORM_STROKES_INPUT, theInput, theInput);
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesWrongSubmitTest)} - empty string")]
    public void StrokesWrongSubmitTest(string theInput, string expectedError)
    {
        // Empty strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventStrokesInvalidAsync, no need to test this case.
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_STROKES_INPUT, theInput);

        WrongSubmitTest(
            theInput,
            expectedError,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            IDs.ALTERNATIVE_FORM_STROKES_ERROR);
    }

    [TestCase("0", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - zero")]
    [TestCase("1", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - one")]
    [TestCase("9", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - nine")]
    [TestCase("13", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - thirteen")]
    [TestCase("66", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - sixty-six")]
    [TestCase("99", TestName = $"{nameof(AlternativeFormTest)}.{nameof(StrokesCorrectSubmitTest)} - ninety-nine")]
    public void StrokesCorrectSubmitTest(string theInput)
    {
        _ = _testContext.JSInterop.Setup<bool>(DOMFunctions.IS_VALID_INPUT, IDs.ALTERNATIVE_FORM_STROKES_INPUT).SetResult(true);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.ALTERNATIVE_FORM_STROKES_INPUT, theInput);

        CorrectSubmitTest(
            theInput,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            IDs.ALTERNATIVE_FORM_STROKES_ERROR);
    }

    private void VerifyInputValueSet(
        IRenderedComponent<AlternativeForm> alternativeFormComponent,
        string inputId,
        string valueToSet,
        string expectedContent)
    {
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, inputId, expectedContent);
        var alternativeFormInput = alternativeFormComponent.Find($"#{inputId}");
        alternativeFormInput.Input(valueToSet);

        var setValueInvocation = setValueInvocationHandler.VerifyInvoke(DOMFunctions.SET_VALUE);
        Assert.That(setValueInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setValueInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(setValueInvocation.Arguments[1], Is.EqualTo(expectedContent));
        _ = setValueInvocationHandler.SetVoidResult();
    }

    private void WrongSubmitTest(
        string theInput,
        string expectedError,
        string inputId,
        string errorDivId)
    {
        var (addClassInvocationHandler, setTextInvocationHandler, alternativeFormInput, alternativeFormSubmitButton) = MockFormElements(inputId, errorDivId, expectedError);
        alternativeFormInput.Input(theInput);
        alternativeFormSubmitButton.Click();

        var addClassInvocation = addClassInvocationHandler.VerifyInvoke(DOMFunctions.ADD_CLASS);
        Assert.That(addClassInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(addClassInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(addClassInvocation.Arguments[1], Is.EqualTo(CssClasses.BORDER_DANGER));

        var setTextInvocation = setTextInvocationHandler.VerifyInvoke(DOMFunctions.SET_TEXT);
        Assert.That(setTextInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setTextInvocation.Arguments[0], Is.EqualTo(errorDivId));
        Assert.That(setTextInvocation.Arguments[1], Is.EqualTo(expectedError));
        _ = addClassInvocationHandler.SetVoidResult();
        _ = setTextInvocationHandler.SetVoidResult();
    }

    private void CorrectSubmitTest(
        string theInput,
        string inputId,
        string errorDivId)
    {
        foreach (var otherInputId in _inputIds)
        {
            if (otherInputId != inputId)
            {
                _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, otherInputId, CssClasses.BORDER_DANGER);
            }
        }

        var (addClassInvocationHandler, setTextInvocationHandler, alternativeFormInput, alternativeFormSubmitButton) = MockFormElements(inputId, errorDivId);
        alternativeFormInput.Input(theInput);
        alternativeFormSubmitButton.Click();

        addClassInvocationHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);
        setTextInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_TEXT);
        _ = addClassInvocationHandler.SetVoidResult();
        _ = setTextInvocationHandler.SetVoidResult();
    }

    private (JSRuntimeInvocationHandler, JSRuntimeInvocationHandler, IElement, IElement) MockFormElements(
        string inputId,
        string errorDivId,
        string expectedError = "")
    {
        var alternativeFormComponent = _testContext.RenderComponent<AlternativeForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var addClassInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, inputId, CssClasses.BORDER_DANGER);
        var setTextInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TEXT, errorDivId, expectedError);
        var alternativeFormInput = alternativeFormComponent.Find($"#{inputId}");
        var alternativeFormSubmitButton = alternativeFormComponent.Find($"#{IDs.ALTERNATIVE_FORM_SUBMIT_BUTTON}");

        return (addClassInvocationHandler, setTextInvocationHandler, alternativeFormInput, alternativeFormSubmitButton);
    }

    public void Dispose() => _testContext?.Dispose();
}
