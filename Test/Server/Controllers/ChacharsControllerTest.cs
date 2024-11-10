using Asciipinyin.Web.Server.Test.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Errors = AsciiPinyin.Web.Server.Test.Constants.Errors;

namespace Asciipinyin.Web.Server.Test.Controllers;

[TestFixture]
internal sealed partial class ChacharsControllerTest
{
    private static readonly Chachar _radicalChachar = new()
    {
        TheCharacter = "雨",
        Pinyin = "yu",
        Ipa = "y:",
        Tone = 3,
        Strokes = 8
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

    private static readonly Alternative _alternative = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = "雨",
        OriginalPinyin = "yu",
        OriginalTone = 3,
        Strokes = 8
    };

    private static readonly Mock<AsciiPinyinContext> _asciiPinyinContextMock = new();
    private static readonly Mock<ILogger<ChacharsController>> _loggerMock = new();

    private HttpContext _httpContext = default!;
    private ChacharsController _chacharsController = default!;

    [SetUp]
    public void SetUp()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Headers[RequestHeaderKeys.USER_AGENT] = "test";

        _chacharsController = new(_asciiPinyinContextMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [TearDown]
    public void TearDown() => _asciiPinyinContextMock.Reset();

    [Test]
    public void PostNoUserAgentHeaderTest()
    {
        _httpContext.Request.Headers.Clear();

        var result = _chacharsController.Post(new Chachar());
        EntityControllerTestCommons.NoUserAgentHeaderTest(result);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public void PostTheCharacterWrongTest(string? theCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            TheCharacter = theCharacter
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, theCharacter, ColumnNames.THE_CHARACTER);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - zero")]
    [TestCase("1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - space")]
    [TestCase("   ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - dot")]
    [TestCase(":", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - colon")]
    [TestCase(";", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostPinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void PostPinyinWrongTest(string? pinyin, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            Pinyin = pinyin
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, pinyin, ColumnNames.PINYIN);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - null")]
    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - fifty-five")]
    [TestCase(byte.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - byte max value")]
    public void PostToneWrongTest(byte? tone, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            Tone = tone
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, tone, ColumnNames.TONE);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - zero")]
    [TestCase("1", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - space")]
    [TestCase("   ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - backslash")]
    [TestCase("\"", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - backtick")]
    [TestCase(";", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("A", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single ASCII character uppercase")]
    [TestCase("ABC", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("ɼ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_IPA, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostIpaWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void PostIpaWrongTest(string? ipa, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            Ipa = ipa
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, ipa, "ipa");
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - null")]
    [TestCase(0, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - zero")]
    [TestCase(100, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - one hundred")]
    [TestCase(byte.MaxValue, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - byte max value")]
    public void PostStrokesWrongTest(byte? strokes, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            Strokes = strokes
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, strokes, "strokes");
    }

    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public void PostRadicalCharacterWrongTest(string? radicalCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalCharacter = radicalCharacter
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, radicalCharacter, ColumnNames.RADICAL_CHARACTER);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.MISSING,
            (null, ColumnNames.RADICAL_PINYIN),
            (null, ColumnNames.RADICAL_TONE)
        );
    }

    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - zero")]
    [TestCase("1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - space")]
    [TestCase("   ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - dot")]
    [TestCase(":", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - colon")]
    [TestCase(";", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalPinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public void PostRadicalPinyinWrongTest(string? radicalPinyin, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalPinyin = radicalPinyin
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, radicalPinyin, ColumnNames.RADICAL_PINYIN);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.MISSING,
            (null, ColumnNames.RADICAL_CHARACTER),
            (null, ColumnNames.RADICAL_TONE)
        );
    }

    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - fifty-five")]
    [TestCase(byte.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - byte max value")]
    public void PostRadicalToneWrongTest(byte? radicalTone, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            RadicalTone = radicalTone
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, radicalTone, ColumnNames.RADICAL_TONE);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.MISSING,
            (null, ColumnNames.RADICAL_CHARACTER),
            (null, ColumnNames.RADICAL_PINYIN)
        );
    }

    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalAlternativeCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public void PostRadicalAlternativeCharacterWrongTest(string? radicalAlternativeCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalAlternativeCharacter = radicalAlternativeCharacter
        };

        var result = _chacharsController.Post(chachar);
        EntityControllerTestCommons.PostFieldWrongTest(result, expectedErrorMessage, radicalAlternativeCharacter, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.MISSING,
            (null, ColumnNames.RADICAL_CHARACTER),
            (null, ColumnNames.RADICAL_PINYIN),
            (null, ColumnNames.RADICAL_TONE)
        );
    }

    [Test]
    public void PostGetAllChacharsErrorTest()
    {
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Throws(new InvalidOperationException());

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.InternalServerErrorTest(result);
    }

    [Test]
    public void PostGetAllAlternativesErrorTest()
    {
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock(_radicalChachar);
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
        _ = _asciiPinyinContextMock.Setup(context => context.Alternatives).Throws(new InvalidOperationException());

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.InternalServerErrorTest(result);
    }

    [Test]
    public void PostRadicalUnknownTest()
    {
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock<Chachar>();
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
        _httpContext.Request.Headers[RequestHeaderKeys.USER_AGENT] = "test";

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.UNKNOWN_CHACHAR,
            (_nonRadicalChacharWithAlternative.RadicalCharacter, ColumnNames.RADICAL_CHARACTER),
            (_nonRadicalChacharWithAlternative.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
            (_nonRadicalChacharWithAlternative.RadicalTone, ColumnNames.RADICAL_TONE)
        );
    }

    [Test]
    public void PostRadicalNotRadicalTest()
    {
        var malformedRadicalChachar = new Chachar()
        {
            TheCharacter = _radicalChachar.TheCharacter,
            Pinyin = _radicalChachar.Pinyin,
            Ipa = _radicalChachar.Ipa,
            Tone = _radicalChachar.Tone,
            Strokes = _radicalChachar.Strokes,
            RadicalCharacter = _radicalChachar.TheCharacter
        };

        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock(malformedRadicalChachar);
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.NO_RADICAL,
            (_nonRadicalChacharWithAlternative.RadicalCharacter, ColumnNames.RADICAL_CHARACTER),
            (_nonRadicalChacharWithAlternative.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
            (_nonRadicalChacharWithAlternative.RadicalTone, ColumnNames.RADICAL_TONE)
        );
    }

    [Test]
    public void PostAlternativeUnknownTest()
    {
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock(_radicalChachar);
        var alternativesDbSetMock = EntityControllerTestCommons.GetDbSetMock<Alternative>();
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
        _ = _asciiPinyinContextMock.Setup(context => context.Alternatives).Returns(alternativesDbSetMock.Object);

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.UNKNOWN_ALTERNATIVE,
            (_nonRadicalChacharWithAlternative.RadicalAlternativeCharacter, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER),
            (_nonRadicalChacharWithAlternative.RadicalCharacter, ColumnNames.RADICAL_CHARACTER),
            (_nonRadicalChacharWithAlternative.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
            (_nonRadicalChacharWithAlternative.RadicalTone, ColumnNames.RADICAL_TONE)
        );
    }

    [Test]
    public void PostChacharAlreadyExistsTest()
    {
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock(_radicalChachar);
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);

        var result = _chacharsController.Post(_radicalChachar);
        EntityControllerTestCommons.PostFieldsWrongTest(
            result,
            Errors.CHACHAR_ALREADY_EXISTS,
            (_radicalChachar.TheCharacter, ColumnNames.THE_CHARACTER),
            (_radicalChachar.Pinyin, ColumnNames.PINYIN),
            (_radicalChachar.Tone, ColumnNames.TONE)
        );
    }

    [Test]
    public void PostRadicalChacharOkTest()
    {
        EntityControllerTestCommons.MockDatabaseFacadeTransaction(_asciiPinyinContextMock);
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock<Chachar>();
        var alternativesDbSetMock = EntityControllerTestCommons.GetDbSetMock<Alternative>();
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
        _ = _asciiPinyinContextMock.Setup(context => context.Alternatives).Returns(alternativesDbSetMock.Object);

        var result = _chacharsController.Post(_radicalChachar);
        EntityControllerTestCommons.PostOkTest(result);
    }

    [Test]
    public void PostNonRadicalChacharOkTest()
    {
        EntityControllerTestCommons.MockDatabaseFacadeTransaction(_asciiPinyinContextMock);
        var chacharsDbSetMock = EntityControllerTestCommons.GetDbSetMock(_radicalChachar);
        var alternativesDbSetMock = EntityControllerTestCommons.GetDbSetMock(_alternative);
        _ = _asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
        _ = _asciiPinyinContextMock.Setup(context => context.Alternatives).Returns(alternativesDbSetMock.Object);

        var result = _chacharsController.Post(_nonRadicalChacharWithAlternative);
        EntityControllerTestCommons.PostOkTest(result);
    }
}
