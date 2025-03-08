using Asciipinyin.Web.Server.Test.Constants;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Test.Commons;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Errors = AsciiPinyin.Web.Server.Test.Constants.Errors;

namespace Asciipinyin.Web.Server.Test.Controllers;

[TestFixture]
internal sealed class ChacharsControllerTest
{
    private const string PATH = $"/{ApiNames.BASE}/{ApiNames.CHARACTERS}";
    private const string PATH_DELETE = $"{PATH}/{ApiNames.DELETE}";

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
        TheCharacter = "儿",
        Pinyin = "er",
        Ipa = "ɚ",
        Tone = 2,
        Strokes = 2
    };

    private static readonly Chachar _radicalChachar3 = new()
    {
        TheCharacter = "辵",
        Pinyin = "chuo",
        Ipa = "ʈʂʰuɔ",
        Tone = 4,
        Strokes = 7
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative11 = new()
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

    private static readonly Chachar _nonRadicalChacharWithoutAlternative21 = new()
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

    private static readonly Chachar _nonRadicalChacharWithoutAlternative22 = new()
    {
        TheCharacter = "先",
        Pinyin = "xian",
        Ipa = "ɕjɛn",
        Tone = 1,
        Strokes = 6,
        RadicalCharacter = "儿",
        RadicalPinyin = "er",
        RadicalTone = 2
    };

    private static readonly Chachar _nonRadicalChacharWithoutAlternative23 = new()
    {
        TheCharacter = "光",
        Pinyin = "guang",
        Ipa = "kwɑŋ",
        Tone = 1,
        Strokes = 6,
        RadicalCharacter = "儿",
        RadicalPinyin = "er",
        RadicalTone = 2
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative31 = new()
    {
        TheCharacter = "这",
        Pinyin = "zhe",
        Ipa = "dʐə",
        Tone = 4,
        Strokes = 7,
        RadicalCharacter = "辵",
        RadicalPinyin = "chuo",
        RadicalTone = 4,
        RadicalAlternativeCharacter = "⻌"
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative32 = new()
    {
        TheCharacter = "过",
        Pinyin = "guo",
        Ipa = "kuɔ",
        Tone = 1,
        Strokes = 6,
        RadicalCharacter = "辵",
        RadicalPinyin = "chuo",
        RadicalTone = 4,
        RadicalAlternativeCharacter = "⻌"
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative33 = new()
    {
        TheCharacter = "道",
        Pinyin = "dao",
        Ipa = "taʊ",
        Tone = 4,
        Strokes = 12,
        RadicalCharacter = "辵",
        RadicalPinyin = "chuo",
        RadicalTone = 4,
        RadicalAlternativeCharacter = "⻌"
    };

    private static readonly Alternative _alternative11 = new()
    {
        TheCharacter = "⻗",
        OriginalCharacter = "雨",
        OriginalPinyin = "yu",
        OriginalTone = 3,
        Strokes = 8
    };

    private static readonly Alternative _alternative31 = new()
    {
        TheCharacter = "⻌",
        OriginalCharacter = "辵",
        OriginalPinyin = "chuo",
        OriginalTone = 4,
        Strokes = 3
    };

    private static readonly Alternative _alternative32 = new()
    {
        TheCharacter = "⻍",
        OriginalCharacter = "辵",
        OriginalPinyin = "chuo",
        OriginalTone = 4,
        Strokes = 4
    };

    private static readonly Alternative _alternative33 = new()
    {
        TheCharacter = "⻎",
        OriginalCharacter = "辵",
        OriginalPinyin = "chuo",
        OriginalTone = 4,
        Strokes = 3
    };

    private static readonly Mock<AsciiPinyinContext> _asciiPinyinContextMock = new(new DbContextOptions<AsciiPinyinContext>());
    private readonly EntityControllerTestCommons<ChacharsController, Chachar> _entityControllerTestCommons = new(PATH, PATH_DELETE, _asciiPinyinContextMock);

    private IHost _host = default!;
    private AsciiPinyinContext _asciiPinyinContext = default!;

    [SetUp]
    public async Task SetUp()
    {
        _host = await _entityControllerTestCommons.SetUpHost();
        _asciiPinyinContext = _host.Services.GetRequiredService<AsciiPinyinContext>();
    }

    [TearDown]
    public void TearDown() => _entityControllerTestCommons.TearDown(_asciiPinyinContext, _host);

    [Test]
    public async Task GetNoUserAgentHeaderTest()
    {
        var response = await _host.GetTestClient().GetAsync(PATH);
        await _entityControllerTestCommons.NoUserAgentHeaderTestAsync(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS)]
    public async Task GetAllChacharsErrorTest()
    {
        var response = await _entityControllerTestCommons.GetAsync(_host, CancellationToken.None);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetAllChacharsOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _nonRadicalChacharWithoutAlternative21
        );

        var response = await _entityControllerTestCommons.GetAsync(_host, CancellationToken.None);

        await _entityControllerTestCommons.GetAllEntitiesOkTestAsync(
            response,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _nonRadicalChacharWithoutAlternative21
        );
    }

    [Test]
    public async Task PostNoUserAgentHeaderTest()
    {
        var response = await _host.GetTestClient().PostAsJsonAsync(PATH, _radicalChachar1);
        await _entityControllerTestCommons.NoUserAgentHeaderTestAsync(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_radicalChachar1));
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
    public async Task PostTheCharacterWrongTest(string? theCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            TheCharacter = theCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
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
    public async Task PostPinyinWrongTest(string? pinyin, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            Pinyin = pinyin
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - null")]
    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - fifty-five")]
    [TestCase(short.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostToneWrongTest)} - short max value")]
    public async Task PostToneWrongTest(short? tone, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            Tone = tone
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
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
    public async Task PostIpaWrongTest(string? ipa, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            Ipa = ipa
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - null")]
    [TestCase(0, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - zero")]
    [TestCase(100, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - one hundred")]
    [TestCase(short.MaxValue, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostStrokesWrongTest)} - short max value")]
    public async Task PostStrokesWrongTest(short? strokes, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            Strokes = strokes
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
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
    public async Task PostRadicalCharacterWrongTest(string? radicalCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalCharacter = radicalCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
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
    public async Task PostRadicalPinyinWrongTest(string? radicalPinyin, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalPinyin = radicalPinyin
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
    }

    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - fifty-five")]
    [TestCase(short.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostRadicalToneWrongTest)} - short max value")]
    public async Task PostRadicalToneWrongTest(short? radicalTone, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            RadicalTone = radicalTone
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
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
    public async Task PostRadicalAlternativeCharacterWrongTest(string? radicalAlternativeCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            RadicalAlternativeCharacter = radicalAlternativeCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(chachar));
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS)]
    public async Task PostGetAllChacharsErrorTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES)]
    public async Task PostGetAllAlternativesErrorTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostRadicalUnknownTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _nonRadicalChacharWithAlternative11, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_nonRadicalChacharWithAlternative11));
    }

    [Test]
    public async Task PostRadicalNotRadicalTest()
    {
        var radicalNotRadical = new Chachar()
        {
            TheCharacter = _radicalChachar1.TheCharacter,
            Pinyin = _radicalChachar1.Pinyin,
            Ipa = _radicalChachar1.Ipa,
            Tone = _radicalChachar1.Tone,
            Strokes = _radicalChachar1.Strokes,
            RadicalCharacter = _radicalChachar1.TheCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, radicalNotRadical, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(radicalNotRadical));
    }

    [Test]
    public async Task PostAlternativeUnknownTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1);
        var response = await _entityControllerTestCommons.PostAsync(_host, _nonRadicalChacharWithAlternative11, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_nonRadicalChacharWithAlternative11));
    }

    [Test]
    public async Task PostChacharAlreadyExistsTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1);
        var response = await _entityControllerTestCommons.PostAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK)]
    public async Task PostChacharSaveFailedTest()
    {
        _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.SaveChanges()).Throws(new InvalidOperationException());
        var response = await _entityControllerTestCommons.PostAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostRadicalChacharOkTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
    }

    [Test]
    public async Task PostNonRadicalChacharOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1, _alternative11);
        var response = await _entityControllerTestCommons.PostAsync(_host, _nonRadicalChacharWithAlternative11, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_nonRadicalChacharWithAlternative11));
    }

    [Test]
    public async Task PostDeleteNoUserAgentHeaderTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1);
        var response = await _host.GetTestClient().PostAsJsonAsync(PATH_DELETE, _radicalChachar1);
        await _entityControllerTestCommons.NoUserAgentHeaderTestAsync(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
    }

    // Malformed chachars can't exist in the database => it makes no sense to test if they're inside after a failed delete.
    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task PostDeleteTheCharacterWrongTest(string? theCharacter, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            TheCharacter = theCharacter
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - zero")]
    [TestCase("1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - space")]
    [TestCase("   ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - dot")]
    [TestCase(":", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - colon")]
    [TestCase(";", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeletePinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public async Task PostDeletePinyinWrongTest(string? pinyin, string expectedErrorMessage)
    {
        var chachar = new Chachar()
        {
            Pinyin = pinyin
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteToneWrongTest)} - null")]
    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteToneWrongTest)} - fifty-five")]
    [TestCase(short.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(ChacharsControllerTest)}.{nameof(PostDeleteToneWrongTest)} - short max value")]
    public async Task PostDeleteToneWrongTest(short? tone, string expectedErrorMessage)
    {
        // Unsigned byte numbers are only reachable inputs.
        var chachar = new Chachar()
        {
            Tone = tone
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, chachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS)]
    public async Task PostDeleteGetAllChacharsErrorTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES)]
    public async Task PostDeleteGetAllAlternativesErrorTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _nonRadicalChacharWithAlternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostDeleteChacharDoesNotExistTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_radicalChachar1));
    }

    [Test]
    public async Task PostDeleteMinimalChacharDoesNotExistTest()
    {
        var minimalChachar = new Chachar
        {
            TheCharacter = _radicalChachar1.TheCharacter,
            Pinyin = _radicalChachar1.Pinyin,
            Tone = _radicalChachar1.Tone
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, minimalChachar, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(minimalChachar));
    }

    [Test]
    public async Task PostDeleteRadicalForOneExistingChachar()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
             _radicalChachar2,
             _nonRadicalChacharWithoutAlternative21
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar2, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar2));
    }

    [Test]
    public async Task PostDeleteRadicalForMultipleExistingChachars()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar2,
            _nonRadicalChacharWithoutAlternative21,
            _nonRadicalChacharWithoutAlternative22,
            _nonRadicalChacharWithoutAlternative23
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar2, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar2));
    }

    [Test]
    public async Task PostDeleteRadicalWithOneExistingAlternative()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar3,
            _alternative31
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar3, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar3));
    }

    [Test]
    public async Task PostDeleteRadicalWithMultipleExistingAlternatives()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar3,
            _alternative31,
            _alternative32,
            _alternative33
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar3, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar3));
    }

    [Test]
    public async Task PostDeleteRadicalForOneExistingChacharWithOneExistingAlternative()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _alternative11
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
    }

    [Test]
    public async Task PostDeleteRadicalForMultipleExistingChacharsWithOneExistingAlternative()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar3,
            _nonRadicalChacharWithAlternative31,
            _nonRadicalChacharWithAlternative32,
            _nonRadicalChacharWithAlternative33,
            _alternative31
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar3, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar3));
    }

    [Test]
    public async Task PostDeleteRadicalForOneExistingChacharWithMultipleExistingAlternatives()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar3,
            _nonRadicalChacharWithAlternative31,
            _alternative31,
            _alternative32,
            _alternative33
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar3, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar3));
    }

    [Test]
    public async Task PostDeleteRadicalForMultipleExistingChacharsWithMultipleExistingAlternatives()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar3,
            _nonRadicalChacharWithAlternative31,
            _nonRadicalChacharWithAlternative32,
            _nonRadicalChacharWithAlternative33,
            _alternative31,
            _alternative32,
            _alternative33
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar3, CancellationToken.None);
        _entityControllerTestCommons.PostBadRequestTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar3));
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK)]
    public async Task PostDeleteChacharSaveFailedTest()
    {
        _ = _asciiPinyinContext.Add(_radicalChachar1);
        _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.SaveChanges()).Throws(new InvalidOperationException());
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostDeleteRadicalChacharOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _radicalChachar1, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_radicalChachar1));
    }

    [Test]
    public async Task PostDeleteNonRadicalChacharOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _alternative11
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _nonRadicalChacharWithAlternative11, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);
        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
        Assert.That(_asciiPinyinContext.Chachars, Does.Not.Contain(_nonRadicalChacharWithAlternative11));
        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }
}
