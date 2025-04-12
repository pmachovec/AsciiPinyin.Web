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
using Errors = AsciiPinyin.Web.Shared.Test.Constants.Errors;

namespace Asciipinyin.Web.Server.Test.Controllers;

[TestFixture]
internal sealed class AlternativesControllerTest
{
    private const string PATH = $"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}";
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
        TheCharacter = "丿",
        Pinyin = "pie",
        Ipa = "pʰie",
        Tone = 3,
        Strokes = 1
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
        RadicalAlternativeCharacter = "⻗"
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
        RadicalAlternativeCharacter = "⻗"
    };

    private static readonly Chachar _nonRadicalChacharWithAlternative13 = new()
    {
        TheCharacter = "雪",
        Pinyin = "ɕɥœ",
        Ipa = "na",
        Tone = 3,
        Strokes = 11,
        RadicalCharacter = _radicalChachar1.TheCharacter,
        RadicalPinyin = _radicalChachar1.Pinyin,
        RadicalTone = _radicalChachar1.Tone,
        RadicalAlternativeCharacter = "⻗"
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
        TheCharacter = "乁",
        OriginalCharacter = _radicalChachar2.TheCharacter,
        OriginalPinyin = _radicalChachar2.Pinyin,
        OriginalTone = _radicalChachar2.Tone,
        Strokes = 1
    };

    private static readonly Alternative _alternative22 = new()
    {
        TheCharacter = "乀",
        OriginalCharacter = _radicalChachar2.TheCharacter,
        OriginalPinyin = _radicalChachar2.Pinyin,
        OriginalTone = _radicalChachar2.Tone,
        Strokes = 1
    };

    private static readonly Mock<AsciiPinyinContext> _asciiPinyinContextMock = new(new DbContextOptions<AsciiPinyinContext>());
    private static readonly EntityControllerTestCommons<AlternativesController, Alternative> _entityControllerTestCommons = new(PATH, PATH_DELETE, _asciiPinyinContextMock);

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

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES)]
    public async Task GetAllAlternativesErrorTest()
    {
        var response = await _entityControllerTestCommons.GetAsync(_host, CancellationToken.None);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetAllAlternativesOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _alternative11,
            _alternative21,
            _alternative22
        );

        var response = await _entityControllerTestCommons.GetAsync(_host, CancellationToken.None);

        await _entityControllerTestCommons.GetAllEntitiesOkTestAsync(
            response,
            _alternative11,
            _alternative21,
            _alternative22
        );
    }

    [Test]
    public async Task PostNoUserAgentHeaderTest()
    {
        var response = await _host.GetTestClient().PostAsJsonAsync(PATH, _alternative11);
        await _entityControllerTestCommons.NoUserAgentHeaderTestAsync(response);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(_alternative11));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task PostTheCharacterWrongTest(string? theCharacter, string expectedError)
    {
        var alternative = new Alternative()
        {
            TheCharacter = theCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.THE_CHARACTER, CancellationToken.None, expectedError);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(alternative));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task PostOriginalCharacterWrongTest(string? originalCharacter, string expectedError)
    {
        var alternative = new Alternative()
        {
            OriginalCharacter = originalCharacter
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_CHARACTER, CancellationToken.None, expectedError);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(alternative));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - zero")]
    [TestCase("1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - space")]
    [TestCase("   ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - dot")]
    [TestCase(":", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - colon")]
    [TestCase(";", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalPinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public async Task PostOriginalPinyinWrongTest(string? originalPinyin, string expectedError)
    {
        var alternative = new Alternative()
        {
            OriginalPinyin = originalPinyin
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_PINYIN, CancellationToken.None, expectedError);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(alternative));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalToneWrongTest)} - null")]
    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalToneWrongTest)} - fifty-five")]
    [TestCase(short.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostOriginalToneWrongTest)} - short max value")]
    public async Task PostOriginalToneWrongTest(short? originalTone, string expectedError)
    {
        // Unsigned byte numbers are only reachable inputs.
        var alternative = new Alternative()
        {
            OriginalTone = originalTone
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_TONE, CancellationToken.None, expectedError);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(alternative));
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostStrokesWrongTest)} - null")]
    [TestCase(0, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostStrokesWrongTest)} - zero")]
    [TestCase(100, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostStrokesWrongTest)} - one hundred")]
    [TestCase(short.MaxValue, Errors.ONE_TO_NINETY_NINE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostStrokesWrongTest)} - short max value")]
    public async Task PostStrokesWrongTest(short? strokes, string expectedError)
    {
        // Unsigned byte numbers are only reachable inputs.
        var alternative = new Alternative()
        {
            Strokes = strokes
        };

        var response = await _entityControllerTestCommons.PostAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.STROKES, CancellationToken.None, expectedError);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(alternative));
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS)]
    public async Task PostGetAllChacharsErrorTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES)]
    public async Task PostGetAllAlternativesErrorTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostOriginalUnknownTest()
    {
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);
        await _entityControllerTestCommons.PostBadRequestTestAsync(response, CancellationToken.None, Errors.ORIGINAL_UNKNOWN);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(_alternative11));
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

        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, radicalNotRadical);
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);

        await _entityControllerTestCommons.PostConflictTestAsync(
            response,
            CancellationToken.None,
            Errors.ORIGINAL_NOT_RADICAL
        );

        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(_alternative11));
    }

    [Test]
    public async Task PostAlternativeAlreadyExistsTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1, _alternative11);
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);

        await _entityControllerTestCommons.PostConflictTestAsync(
            response,
            CancellationToken.None,
            Errors.ALTERNATIVE_EXISTS
        );

        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }

    [Test]
    public async Task PostNonKeyFieldDiffersAlternativeAlreadyExistsTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative11.TheCharacter,
            OriginalCharacter = _alternative11.OriginalCharacter,
            OriginalPinyin = _alternative11.OriginalPinyin,
            OriginalTone = _alternative11.OriginalTone,
            Strokes = (short)(_alternative11.Strokes! + 1)
        };

        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1, _alternative11);
        var response = await _entityControllerTestCommons.PostAsync(_host, alternativeClone, CancellationToken.None);

        await _entityControllerTestCommons.PostConflictTestAsync(
            response,
            CancellationToken.None,
            Errors.ALTERNATIVE_EXISTS
        );

        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(alternativeClone));
        // The clone is not in the database, but the presence is decided only by key fields. This is expected state.
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK)]
    public async Task PostAlternativeSaveFailedTest()
    {
        _ = _asciiPinyinContext.Add(_radicalChachar1);
        _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.SaveChanges()).Throws(new InvalidOperationException());
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostAlternativeOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1);
        var response = await _entityControllerTestCommons.PostAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }

    [Test]
    public async Task PostDeleteNoUserAgentHeaderTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1, _alternative11);
        var response = await _host.GetTestClient().PostAsJsonAsync(PATH_DELETE, _alternative11);
        await _entityControllerTestCommons.NoUserAgentHeaderTestAsync(response);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }

    // Malformed alternatives can't exist in the database => it makes no sense to test if they're inside after a failed delete.
    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteTheCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task PostDeleteTheCharacterWrongTest(string? theCharacter, string expectedError)
    {
        var alternative = new Alternative()
        {
            TheCharacter = theCharacter
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.THE_CHARACTER, CancellationToken.None, expectedError);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - empty string")]
    [TestCase("-1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple digits negative float")]
    [TestCase("   ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple spaces")]
    [TestCase("\n\n\n", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple new lines")]
    [TestCase("\t\t\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - space, new line and tabular together")]
    [TestCase("{0}", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("abc", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple ASCII characters lowercase")]
    [TestCase("ABC", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple ASCII characters uppercase")]
    [TestCase("AbCdE", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple ASCII characters case combination")]
    [TestCase("This is an ASCII text", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("zhōng", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("žščřďťň", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("джлщыюя", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝r̻̝r̝̊", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("大考验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.ONLY_ONE_CHARACTER_ALLOWED, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    [TestCase("0", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - zero")]
    [TestCase("1", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single digit positive integer")]
    [TestCase(" ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - space")]
    [TestCase("\n", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - new line")]
    [TestCase("\t", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - tabluar")]
    [TestCase("\\", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - dot")]
    [TestCase(":", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - colon")]
    [TestCase(";", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - closing curly bracket")]
    [TestCase("a", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single ASCII character lowercase")]
    [TestCase("A", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single ASCII character uppercase")]
    [TestCase("ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single pinyin character uppercase")]
    [TestCase("ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("r̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_SINGLE_CHINESE, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalCharacterWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    public async Task PostDeleteOriginalCharacterWrongTest(string? originalCharacter, string expectedError)
    {
        var alternative = new Alternative()
        {
            OriginalCharacter = originalCharacter
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_CHARACTER, CancellationToken.None, expectedError);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - null")]
    [TestCase("", Errors.EMPTY, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - empty string")]
    [TestCase("0", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - zero")]
    [TestCase("1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single digit positive integer")]
    [TestCase("-1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single digit negative integer")]
    [TestCase("123", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple digits positive integer")]
    [TestCase("-123", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple digits negative integer")]
    [TestCase("0.1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - two digits positive float")]
    [TestCase("-0.1", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - two digits negative float")]
    [TestCase("123.456", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple digits positive float")]
    [TestCase("-123.456", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple digits negative float")]
    [TestCase(" ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - space")]
    [TestCase("   ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple spaces")]
    [TestCase("\n", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - new line")]
    [TestCase("\n\n\n", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple new lines")]
    [TestCase("\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - tabluar")]
    [TestCase("\t\t\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple tabulars")]
    [TestCase(" \n\t", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - space, new line and tabular together")]
    [TestCase("\\", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - backslash")]
    [TestCase("\'", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - apostrophe")]
    [TestCase("\"", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - quotes")]
    [TestCase("`", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - backtick")]
    [TestCase(".", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - dot")]
    [TestCase(":", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - colon")]
    [TestCase(";", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - semicolon")]
    [TestCase("@", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - at sign")]
    [TestCase("#", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - hash sign")]
    [TestCase("$", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - dollar sign")]
    [TestCase("{", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - opening curly bracket")]
    [TestCase("}", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - closing curly bracket")]
    [TestCase("{0}", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - placeholder")]
    [TestCase("${@}#\'\"\\ \n\t`.:;", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - ${{@}}#\\\'\\\"\\\\ \\n\\t`.:;")]
    [TestCase("This is an ASCII text", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - ASCII text with spaces")]
    [TestCase("This\nis\nan\nASCII\ntext", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - ASCII text with new lines")]
    [TestCase("This\tis\tan\tASCII\ttext", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - ASCII text with tabulars")]
    [TestCase("This\nis\tan ASCII text ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - ASCII text with new line, tabular and trailing space")]
    [TestCase("ā", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single pinyin character lowercase")]
    [TestCase("Ā", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single pinyin character uppercase")]
    [TestCase("zhōng", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin single syllable lowercase")]
    [TestCase("ZHŌNG", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin single syllable uppercase")]
    [TestCase("ZhŌnG", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin single syllable case combination")]
    [TestCase("dàkǎoyàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables lowercase")]
    [TestCase("DÀKǍOYÀN", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables uppercase")]
    [TestCase("DàKǎOyÀn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables case combination")]
    [TestCase("dà kǎo yàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables lowercase with spaces")]
    [TestCase("DÀ KǍO YÀN", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables uppercase with spaces")]
    [TestCase("Dà KǎO yÀn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables case combination with spaces")]
    [TestCase("Dà\nKǎo\nYàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables with new lines")]
    [TestCase("Dà\tKǎo\tYàn", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables with tabulars")]
    [TestCase("Dà\tKǎo\tYàn ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - pinyin multiple syllables with new line, tabular and space")]
    [TestCase("ř", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Czech non-ASCII character lowercase")]
    [TestCase("Ř", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Czech non-ASCII character uppercase")]
    [TestCase("žščřďťň", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters lowercase")]
    [TestCase("ŽŠČŘĎŤŇ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters uppercase")]
    [TestCase("ŽšČřĎťŇ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Czech non-ASCII characters case combination")]
    [TestCase("Příliš žluťoučký kůň úpěl ďábelské ódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text with spaces")]
    [TestCase("Příliš\nžluťoučký\nkůň\núpěl\nďábelské\nódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text with new lines")]
    [TestCase("Příliš\tžluťoučký\tkůň\túpěl\tďábelské\tódy", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text with tabulars")]
    [TestCase("Příliš\nžluťoučký\tkůň úpěl ďábelské ódy ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text with new line, tabular and trailing space")]
    [TestCase("я", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Russian non-ASCII character lowercase")]
    [TestCase("Я", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Russian non-ASCII character uppercase")]
    [TestCase("джлщыюя", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters lowercase")]
    [TestCase("ДЖЛЩЫЮЯ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters uppercase")]
    [TestCase("ДжЛщЫюЯ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Russian non-ASCII characters case combination")]
    [TestCase("Слишком желтая лошадь ржала дьявольские оды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Russian text with spaces")]
    [TestCase("Слишком\nжелтая\nлошадь\nржала\nдьявольские\nоды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Russian text with new lines")]
    [TestCase("Слишком\tжелтая\tлошадь\tржала\tдьявольские\tоды", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Russian text with tabulars")]
    [TestCase("Слишком\nжелтая\tлошадь ржала дьявольские оды ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Russian text with new line, tabular and trailing space")]
    [TestCase("r̝", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 1")]
    [TestCase("r̻̝", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 2")]
    [TestCase("r̝̊", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - version 3")]
    [TestCase("ɼ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - deprecated version")]
    [TestCase("r̝r̻̝r̝̊", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech 'Ř' in IPA - versions 1, 2 and 3 together")]
    [TestCase("ʈʂʊŋ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - '中' in IPA")]
    [TestCase("pr̝i:liʃʒlucɔutʃki:ku:ɲu:pjɛlɟa:bɛlskɛ:ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text in IPA")]
    [TestCase("pr̝i:liʃ ʒlucɔutʃki: ku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text in IPA with spaces")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\nku:ɲ\nu:pjɛl\nɟa:bɛlskɛ:\nɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text in IPA with new lines")]
    [TestCase("pr̝i:liʃ\tʒlucɔutʃki:\tku:ɲ\tu:pjɛl\tɟa:bɛlskɛ:\tɔ:di", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text in IPA with tabulars")]
    [TestCase("pr̝i:liʃ\nʒlucɔutʃki:\tku:ɲ u:pjɛl ɟa:bɛlskɛ: ɔ:di ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - Czech text in IPA with new line, tabular and trailing space")]
    [TestCase("中", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK unified ideographs")]
    [TestCase("⺫", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK radicals supplement")]
    [TestCase("㆕", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - Kanbun")]
    [TestCase("晴", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs")]
    [TestCase("輸", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK compatibility ideographs supplement")]
    [TestCase("㒡", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension A")]
    [TestCase("𥒯", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension B")]
    [TestCase("𫇂", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension C")]
    [TestCase("𫟖", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension D")]
    [TestCase("𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension E")]
    [TestCase("𭕄", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension F")]
    [TestCase("\U000310f9", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - single Chinese character - CJK extension G")]
    [TestCase("大考验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs")]
    [TestCase("大 考 验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with spaces")]
    [TestCase("大\n考\n验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new lines")]
    [TestCase("大\t考\t验", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with tabulars")]
    [TestCase("大\t考\t验 ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK unified ideographs with new line, tabular and space")]
    [TestCase("𫇂𫟖𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination")]
    [TestCase("𫇂 𫟖 𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with spaces")]
    [TestCase("𫇂\n𫟖\n𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new lines")]
    [TestCase("𫇂\t𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with tabulars")]
    [TestCase("𫇂\n𫟖\t𬩽 ", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - multiple Chinese characters - CJK extensions combination with new line, tabular and space")]
    [TestCase("0-1${@}#'\"\\`.:;aAāĀřŘяЯr̝r̻̝r̝̊中⺫㆕   大考验𫇂\n𫟖\t𬩽", Errors.NO_ASCII, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalPinyinWrongTest)} - crazy combination of 40 characters, symbols and whitespaces")]
    public async Task PostDeleteOriginalPinyinWrongTest(string? originalPinyin, string expectedError)
    {
        var alternative = new Alternative()
        {
            OriginalPinyin = originalPinyin
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_PINYIN, CancellationToken.None, expectedError);
    }

    [TestCase(null, Errors.MISSING, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalToneWrongTest)} - null")]
    [TestCase(5, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalToneWrongTest)} - five")]
    [TestCase(55, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalToneWrongTest)} - fifty-five")]
    [TestCase(short.MaxValue, Errors.ZERO_TO_FOUR, TestName = $"{nameof(AlternativesControllerTest)}.{nameof(PostDeleteOriginalToneWrongTest)} - short max value")]
    public async Task PostDeleteOriginalToneWrongTest(short? originalTone, string expectedError)
    {
        // Unsigned byte numbers are only reachable inputs.
        var alternative = new Alternative()
        {
            OriginalTone = originalTone
        };

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, alternative, CancellationToken.None);
        await _entityControllerTestCommons.PostWrongFieldTestAsync(response, JsonPropertyNames.ORIGINAL_TONE, CancellationToken.None, expectedError);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS)]
    public async Task PostDeleteGetAllChacharsErrorTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES)]
    public async Task PostDeleteGetAllAlternativesErrorTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostDeleteAlternativeDoesNotExistTest()
    {
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);
        await _entityControllerTestCommons.PostBadRequestTestAsync(response, CancellationToken.None, Errors.ALTERNATIVE_UNKNOWN);
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(_alternative11));
    }

    [Test]
    public async Task PostDeleteStrokesDifferAlternativeDoesNotExistTest()
    {
        var alternativeClone = new Alternative
        {
            TheCharacter = _alternative11.TheCharacter,
            OriginalCharacter = _alternative11.OriginalCharacter,
            OriginalPinyin = _alternative11.OriginalPinyin,
            OriginalTone = _alternative11.OriginalTone,
            Strokes = (short)(_alternative11.Strokes! + 1)
        };

        _entityControllerTestCommons.AddToContextAndSave(_asciiPinyinContext, _radicalChachar1, _alternative11);
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, alternativeClone, CancellationToken.None);
        await _entityControllerTestCommons.PostBadRequestTestAsync(response, CancellationToken.None, Errors.ALTERNATIVE_UNKNOWN);

        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(alternativeClone));
        // The clone is not in the database, but the presence is decided only by key fields. This is expected state.
    }

    [Test]
    public async Task PostDeleteAlternativeForOneExistingChachar()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _alternative11
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);

        await _entityControllerTestCommons.PostConflictTestAsync(
            response,
            CancellationToken.None,
            Errors.IS_ALTERNATIVE_FOR_CHACHARS
        );

        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }

    [Test]
    public async Task PostDeleteAlternativeForMultipleExistingChachars()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _nonRadicalChacharWithAlternative11,
            _nonRadicalChacharWithAlternative12,
            _nonRadicalChacharWithAlternative13,
            _alternative11
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);

        await _entityControllerTestCommons.PostConflictTestAsync(
            response,
            CancellationToken.None,
            Errors.IS_ALTERNATIVE_FOR_CHACHARS
        );

        Assert.That(_asciiPinyinContext.Alternatives, Does.Contain(_alternative11));
    }

    [Test, Category(TestCategories.DB_CONTEXT_MOCK)]
    public async Task PostDeleteAlternativeSaveFailedTest()
    {
        _ = _asciiPinyinContext.Add(_radicalChachar1);
        _ = _asciiPinyinContext.Add(_alternative11);
        _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.SaveChanges()).Throws(new InvalidOperationException());
        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.InternalServerErrorTest(response);
    }

    [Test]
    public async Task PostDeleteAlternativeOkTest()
    {
        _entityControllerTestCommons.AddToContextAndSave(
            _asciiPinyinContext,
            _radicalChachar1,
            _alternative11
        );

        var response = await _entityControllerTestCommons.PostDeleteAsync(_host, _alternative11, CancellationToken.None);
        _entityControllerTestCommons.PostOkTest(response);

        Assert.That(_asciiPinyinContext.Chachars, Does.Contain(_radicalChachar1));
        Assert.That(_asciiPinyinContext.Alternatives, Does.Not.Contain(_alternative11));
    }
}
