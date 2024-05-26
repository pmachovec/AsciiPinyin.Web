using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
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

namespace Asciipinyin.Web.Client.Test.Pages.IndexComponents.ChacharsTabComponents;

[TestFixture]
internal sealed class ChacharFormTest : IDisposable
{
    private const string COMPULSORY_VALUE = nameof(COMPULSORY_VALUE);
    private const string MUST_BE_CHINESE_CHARACTER = nameof(MUST_BE_CHINESE_CHARACTER);
    private const string ONLY_ASCII_ALLOWED = nameof(ONLY_ASCII_ALLOWED);

    private readonly Index _indexMock = Mock.Of<Index>();
    private readonly IStringLocalizer<Resource> _localizerMock = Mock.Of<IStringLocalizer<Resource>>();

    private TestContext _testContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _ = Mock.Get(_localizerMock).Setup(localizer => localizer[Resource.CompulsoryValue]).Returns(new LocalizedString(COMPULSORY_VALUE, COMPULSORY_VALUE));
        _ = Mock.Get(_localizerMock).Setup(localizer => localizer[Resource.MustBeChineseCharacter]).Returns(new LocalizedString(MUST_BE_CHINESE_CHARACTER, MUST_BE_CHINESE_CHARACTER));
        _ = Mock.Get(_localizerMock).Setup(localizer => localizer[Resource.OnlyAsciiAllowed]).Returns(new LocalizedString(ONLY_ASCII_ALLOWED, ONLY_ASCII_ALLOWED));
    }

    [SetUp]
    public void SetUp()
    {
        var alternativeSelectorMock = Mock.Of<EntitySelector<Alternative>>();
        var radicalSelectorMock = Mock.Of<EntitySelector<Chachar>>();

        _testContext = new TestContext();

        _ = _testContext.ComponentFactories.Add(alternativeSelectorMock);
        _ = _testContext.ComponentFactories.Add(radicalSelectorMock);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.DISABLE, IDs.CHACHAR_FORM_ALTERNATIVE);
        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, string.Empty);
        _ = _testContext.Services.AddSingleton(_localizerMock);
        _ = _testContext.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();
    }

    [TearDown]
    public void TearDown() => Dispose();

    [TestCase("", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - empty string")]
    [TestCase("0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - zero as string")]
    [TestCase("1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - single digit positive integer as string")]
    [TestCase(" ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - space")]
    [TestCase("\n", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - new line")]
    [TestCase("\t", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - tabluar")]
    [TestCase("\\", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - backslash")]
    [TestCase("\'", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - apostrophe")]
    [TestCase("\"", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputUnchangedTest)} - quotes")]
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
    public void TheCharacterOnInputUnchangedTest(string theInput)
    {
        var chacharFormComponent = _testContext.RenderComponent<ChacharForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, theInput);
        var chacharFormTheCharacterInput = chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_THE_CHARACTER_INPUT}");
        chacharFormTheCharacterInput.Change(theInput);

        setValueInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_VALUE);
    }

    [TestCase("true", "t", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - true as string")]
    [TestCase("false", "f", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - false as string")]
    [TestCase("-1", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - single digit negative integer as string")]
    [TestCase("123", "1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits positive integer as string")]
    [TestCase("-123", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple digits negative integer as string")]
    [TestCase("0.1", "0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits positive float as string")]
    [TestCase("-0.1", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits negative float as string")]
    [TestCase("123.456", "1", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits positive float as string")]
    [TestCase("-123.456", "-", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - two digits negative float as string")]
    [TestCase("   ", " ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple spaces")]
    [TestCase("\n\n\n", "\n", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple new lines")]
    [TestCase("\t\t\t", "\t", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple tabulars")]
    [TestCase(" \n\t", " ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - space, new line and tabular together")]
    [TestCase("{0}", "{", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - placeholder")]
    [TestCase("${@}#\'\"\\", "$", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - ${{@}}#\'\"\\")]
    [TestCase("abc", "a", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", "A", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", "A", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple ASCII characters case combination")]
    [TestCase("This is a longer ASCII text", "T", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - longer ASCII text with spaces")]
    [TestCase("zhōng", "z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", "Z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", "Z", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", "d", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", "d", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with new lines")]
    [TestCase("Dà\tKǎo\tYàn", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", "D", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - pinyin multiple syllables lowercase with new line, tabular and space")]
    [TestCase("žščřďťň", "ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", "Ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", "Ž", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", "P", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - longer Czech text with spaces")]
    [TestCase("джлщыюя", "д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", "Д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", "Д", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", "С", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - longer russian text with spaces")]
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
    [TestCase("0-1${@}#'\"\\aAāĀřŘяЯ中⺫㆕   𫇂\n𫟖\t𬩽", "0", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterOnInputAdjustedTest)} - crazy combination of 30 characters, symbols and whitespaces")]
    public void TheCharacterOnInputAdjustedTest(string theInput, string expectedContent)
    {
        var chacharFormComponent = _testContext.RenderComponent<ChacharForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));

        var setValueInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, expectedContent);
        var chacharFormTheCharacterInput = chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_THE_CHARACTER_INPUT}");
        chacharFormTheCharacterInput.Change(theInput);

        var setValueInvocation = setValueInvocationHandler.VerifyInvoke(DOMFunctions.SET_VALUE);
        Assert.That(setValueInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setValueInvocation.Arguments[0], Is.EqualTo(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT));
        Assert.That(setValueInvocation.Arguments[1], Is.EqualTo(expectedContent));
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - empty string")]
    [TestCase("true", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - true as string")]
    [TestCase("false", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - false as string")]
    [TestCase("0", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - zero as string")]
    [TestCase("1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single digit positive integer as string")]
    [TestCase("-1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single digit negative integer as string")]
    [TestCase("123", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple digits positive integer as string")]
    [TestCase("-123", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple digits negative integer as string")]
    [TestCase("0.1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - two digits positive float as string")]
    [TestCase("-0.1", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - two digits negative float as string")]
    [TestCase("123.456", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - two digits positive float as string")]
    [TestCase("-123.456", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - two digits negative float as string")]
    [TestCase(" ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - space")]
    [TestCase("   ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple spaces")]
    [TestCase("\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - new line")]
    [TestCase("\n\n\n", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple new lines")]
    [TestCase("\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - tabluar")]
    [TestCase("\t\t\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple tabulars")]
    [TestCase(" \n\t", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - space, new line and tabular together")]
    [TestCase("\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - backslash")]
    [TestCase("\'", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - apostrophe")]
    [TestCase("\"", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - quotes")]
    [TestCase("@", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - at sign")]
    [TestCase("#", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - hash sign")]
    [TestCase("$", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - dollar sign")]
    [TestCase("{", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - closing curly bracket")]
    [TestCase("{0}", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - placeholder")]
    [TestCase("${@}#\'\"\\", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - ${{@}}#\'\"\\")]
    [TestCase("a", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character lowercase")]
    [TestCase("A", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single ASCII character uppercase")]
    [TestCase("abc", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple ASCII characters case combination")]
    [TestCase("This is a longer ASCII text", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - longer ASCII text with spaces")]
    [TestCase("ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character lowercase")]
    [TestCase("Ā", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables lowercase with new lines")]
    [TestCase("Dà\tKǎo\tYàn", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables lowercase with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - pinyin multiple syllables lowercase with new line, tabular and space")]
    [TestCase("ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - longer Czech text with spaces")]
    [TestCase("я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - longer russian text with spaces")]
    [TestCase("0-1${@}#'\"\\aAāĀřŘяЯ中⺫㆕   𫇂\n𫟖\t𬩽", MUST_BE_CHINESE_CHARACTER, TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterWrongSubmitTest)} - crazy combination of 30 characters, symbols and whitespaces")]
    public void TheCharacterWrongSubmitTest(string theInput, string expectedError)
    {
        // Multi-character inputs are cut to the first character thanks to PreventMultipleCharacters.

        if (TextUtils.GetStringRealLength(theInput) > 1)
        {
            _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, TextUtils.GetStringFirstCharacterAsString(theInput));
        }

        WrongSubmitTest(theInput, expectedError, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_ERROR);
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
    [TestCase("大考验", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", TestName = $"{nameof(ChacharFormTest)}.{nameof(TheCharacterCorrectSubmitTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    public void TheCharacterCorrectSubmitTest(string theInput)
    {
        // Multi-character inputs are cut to the first character thanks to PreventMultipleCharacters.

        _ = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_VALUE, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, TextUtils.GetStringFirstCharacterAsString(theInput));
        CorrectSubmitTest(
            theInput,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            IDs.CHACHAR_FORM_THE_CHARACTER_ERROR,
            IDs.CHACHAR_FORM_PINYIN_INPUT);
    }

    [TestCase("", COMPULSORY_VALUE, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - empty string")]
    [TestCase("0", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - zero as string")]
    [TestCase("1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single digit positive integer as string")]
    [TestCase("-1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single digit negative integer as string")]
    [TestCase("123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits positive integer as string")]
    [TestCase("-123", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple digits negative integer as string")]
    [TestCase("0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits positive float as string")]
    [TestCase("-0.1", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits negative float as string")]
    [TestCase("123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits positive float as string")]
    [TestCase("-123.456", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - two digits negative float as string")]
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
    [TestCase("@", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - at sign")]
    [TestCase("#", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - hash sign")]
    [TestCase("$", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - dollar sign")]
    [TestCase("{", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - opening curly bracket")]
    [TestCase("}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - closing curly bracket")]
    [TestCase("{0}", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - placeholder")]
    [TestCase("${@}#\'\"\\", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - ${{@}}#\'\"\\")]
    [TestCase("This is a longer ASCII text", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - longer ASCII text with spaces")]
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
    [TestCase("Dà\nKǎo\nYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables lowercase with new lines")]
    [TestCase("Dà\tKǎo\tYàn", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables lowercase with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - pinyin multiple syllables lowercase with new line, tabular and space")]
    [TestCase("ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - longer Czech text with spaces")]
    [TestCase("я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - longer russian text with spaces")]
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
    [TestCase("0-1${@}#'\"\\aAāĀřŘяЯ中⺫㆕   𫇂\n𫟖\t𬩽", ONLY_ASCII_ALLOWED, TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinWrongSubmitTest)} - crazy combination of 30 characters, symbols and whitespaces")]
    public void PinyinWrongSubmitTest(string theInput, string expectedError) =>
        WrongSubmitTest(theInput, expectedError, IDs.CHACHAR_FORM_PINYIN_INPUT, IDs.CHACHAR_FORM_PINYIN_ERROR);

    [TestCase("true", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - true as string")]
    [TestCase("false", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - false as string")]
    [TestCase("a",TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - single ASCII character lowercase")]
    [TestCase("A",TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - single ASCII character uppercase")]
    [TestCase("abc", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", TestName = $"{nameof(ChacharFormTest)}.{nameof(PinyinCorrectSubmitTest)} - multiple ASCII characters case combination")]
    public void PinyinCorrectSubmitTest(string theInput) =>
        CorrectSubmitTest(
            theInput,
            IDs.CHACHAR_FORM_PINYIN_INPUT,
            IDs.CHACHAR_FORM_PINYIN_ERROR,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT);

    private void WrongSubmitTest(string theInput, string expectedError, string inputId, string errorDivId)
    {
        var chacharFormComponent = _testContext.RenderComponent<ChacharForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var addClassInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, inputId, CssClasses.BORDER_DANGER);
        var setTextInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TEXT, errorDivId, expectedError);
        var chacharFormInput = chacharFormComponent.Find($"#{inputId}");
        var chacharFormSubmitButton = chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_SUBMIT_BUTTON}");
        chacharFormInput.Change(theInput);
        chacharFormSubmitButton.Click();

        var addClassInvocation = addClassInvocationHandler.VerifyInvoke(DOMFunctions.ADD_CLASS);
        Assert.That(addClassInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(addClassInvocation.Arguments[0], Is.EqualTo(inputId));
        Assert.That(addClassInvocation.Arguments[1], Is.EqualTo(CssClasses.BORDER_DANGER));

        var setTextInvocation = setTextInvocationHandler.VerifyInvoke(DOMFunctions.SET_TEXT);
        Assert.That(setTextInvocation.Arguments.Count, Is.EqualTo(2));
        Assert.That(setTextInvocation.Arguments[0], Is.EqualTo(errorDivId));
        Assert.That(setTextInvocation.Arguments[1], Is.EqualTo(expectedError));
    }

    private void CorrectSubmitTest(string theInput, string inputId, string errorDivId, params string[] otherInputIds)
    {
        foreach (var otherInputId in otherInputIds)
        {
            _ = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, otherInputId, CssClasses.BORDER_DANGER);
        }

        var chacharFormComponent = _testContext.RenderComponent<ChacharForm>(parameters => parameters.Add(parameter => parameter.Index, _indexMock));
        var addClassInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.ADD_CLASS, inputId, CssClasses.BORDER_DANGER);
        var setTextInvocationHandler = _testContext.JSInterop.SetupVoid(DOMFunctions.SET_TEXT, errorDivId, It.IsAny<string>());
        var chacharFormInput = chacharFormComponent.Find($"#{inputId}");
        var chacharFormSubmitButton = chacharFormComponent.Find($"#{IDs.CHACHAR_FORM_SUBMIT_BUTTON}");
        chacharFormInput.Change(theInput);
        chacharFormSubmitButton.Click();

        addClassInvocationHandler.VerifyNotInvoke(DOMFunctions.ADD_CLASS);
        setTextInvocationHandler.VerifyNotInvoke(DOMFunctions.SET_TEXT);
    }

    public void Dispose() => _testContext?.Dispose();
}
