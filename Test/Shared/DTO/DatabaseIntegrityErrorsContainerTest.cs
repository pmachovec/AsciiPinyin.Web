using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using NUnit.Framework;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Test.DTO;

[TestFixture]
internal sealed class DatabaseIntegrityErrorsContainerTest
{
    private const string TEST_ERROR_MESSAGE_1 = "test error message 1";
    private const string TEST_ERROR_MESSAGE_2 = "test error message 2";
    private const string TEST_ERROR_MESSAGE_3 = "test error message 3";
    private const string TEST_ERROR_MESSAGE_4 = "test error message 4";

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

    [Test]
    public void EmptyContainerToStringTest()
    {
        var databaseIntegrityErrorsContainer = new DatabaseIntegrityErrorsContainer();
        var result = databaseIntegrityErrorsContainer.ToString();
        Assert.That(result, Is.EqualTo($@"{{""{JsonPropertyNames.DATABASE_INTEGRITY_ERRORS}"":[]}}"));
    }

    [Test]
    public void SingleRadicalChacharNoConflictsToStringTest() =>
        SingleEntityToStringTest(_radicalChachar, TEST_ERROR_MESSAGE_1);

    [Test]
    public void SingleNoRadicalChacharWithAlternativeNoConflictsToStringTest() =>
        SingleEntityToStringTest(_nonRadicalChacharWithAlternative, TEST_ERROR_MESSAGE_1);

    [Test]
    public void SingleNoRadicalChacharWithoutAlternativeNoConflictsToStringTest() =>
        SingleEntityToStringTest(_nonRadicalChacharWithoutAlternative, TEST_ERROR_MESSAGE_1);

    [Test]
    public void SingleAlternativeNoConflictsToStringTest() =>
        SingleEntityToStringTest(_alternative, TEST_ERROR_MESSAGE_1);

    [Test]
    public void SingleRadicalChacharSelfConflictToStringTest() =>
        SingleEntityToStringTest(
            _radicalChachar,
            TEST_ERROR_MESSAGE_1,
            _radicalChachar
        );

    [Test]
    public void SingleNoRadicalChacharWithAlternativeSelfConflictToStringTest() =>
        SingleEntityToStringTest(
            _nonRadicalChacharWithAlternative,
            TEST_ERROR_MESSAGE_1,
            _nonRadicalChacharWithAlternative
        );

    [Test]
    public void SingleNoRadicalChacharWithoutAlternativeSelfConflictToStringTest() =>
        SingleEntityToStringTest(
            _nonRadicalChacharWithoutAlternative,
            TEST_ERROR_MESSAGE_1,
            _nonRadicalChacharWithoutAlternative
        );

    [Test]
    public void SingleAlternativeSelfConflictToStringTest() =>
        SingleEntityToStringTest(
            _alternative,
            TEST_ERROR_MESSAGE_1,
            _alternative
        );

    [Test]
    public void SingleRadicalChacharMultipleConflictsToStringTest() =>
        SingleEntityToStringTest(
            _radicalChachar,
            TEST_ERROR_MESSAGE_1,
            _radicalChachar,
            _nonRadicalChacharWithAlternative,
            _nonRadicalChacharWithoutAlternative,
            _alternative
        );

    [Test]
    public void SingleNoRadicalChacharWithAlternativeMultipleConflictsToStringTest() =>
        SingleEntityToStringTest(
            _nonRadicalChacharWithAlternative,
            TEST_ERROR_MESSAGE_1,
            _radicalChachar,
            _nonRadicalChacharWithAlternative,
            _nonRadicalChacharWithoutAlternative,
            _alternative
        );

    [Test]
    public void SingleNoRadicalChacharWithoutAlternativeMultipleConflictsToStringTest() =>
        SingleEntityToStringTest(
            _nonRadicalChacharWithoutAlternative,
            TEST_ERROR_MESSAGE_1,
            _radicalChachar,
            _nonRadicalChacharWithAlternative,
            _nonRadicalChacharWithoutAlternative,
            _alternative
        );

    [Test]
    public void SingleAlternativeMultipleConflictsToStringTest() =>
        SingleEntityToStringTest(
            _alternative,
            TEST_ERROR_MESSAGE_1,
             _radicalChachar,
             _nonRadicalChacharWithAlternative,
             _nonRadicalChacharWithoutAlternative,
            _alternative
        );

    [Test]
    public void MultipleEntitiesNoConflictsToStringTest()
    {
        ToStringTest(
            (_radicalChachar, TEST_ERROR_MESSAGE_1, []),
            (_nonRadicalChacharWithAlternative, TEST_ERROR_MESSAGE_2, []),
            (_nonRadicalChacharWithoutAlternative, TEST_ERROR_MESSAGE_3, []),
            (_alternative, TEST_ERROR_MESSAGE_4, [])
        );
    }

    [Test]
    public void MultipleEntitiesSelfConflictsToStringTest()
    {
        ToStringTest(
            (_radicalChachar, TEST_ERROR_MESSAGE_1, [_radicalChachar]),
            (_nonRadicalChacharWithAlternative, TEST_ERROR_MESSAGE_2, [_nonRadicalChacharWithAlternative]),
            (_nonRadicalChacharWithoutAlternative, TEST_ERROR_MESSAGE_3, [_nonRadicalChacharWithoutAlternative]),
            (_alternative, TEST_ERROR_MESSAGE_4, [_alternative])
        );
    }

    [Test]
    public void MultipleEntitiesMultipleConflictsToStringTest()
    {
        ToStringTest(
            (
                _radicalChachar,
                TEST_ERROR_MESSAGE_1,
                [_radicalChachar, _nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative, _alternative]
            ),
            (
                _nonRadicalChacharWithAlternative,
                TEST_ERROR_MESSAGE_2,
                [_radicalChachar, _nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative, _alternative]
            ),
            (
                _nonRadicalChacharWithoutAlternative,
                TEST_ERROR_MESSAGE_3,
                [_radicalChachar, _nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative, _alternative]
            ),
            (
                _alternative,
                TEST_ERROR_MESSAGE_4,
                [_radicalChachar, _nonRadicalChacharWithAlternative, _nonRadicalChacharWithoutAlternative, _alternative]
            )
        );
    }

    private static void SingleEntityToStringTest(
        IEntity entity,
        string errorMessage,
        params IEntity[] entitiesInConflict
    ) => ToStringTest((entity, errorMessage, entitiesInConflict));

    private static void ToStringTest(params (IEntity, string, IEnumerable<IEntity>)[] databaseIntegrityErrorsData)
    {
        var databaseIntegrityErrorsContainerRegex = GetDatabaseIntegrityErrorsContainerRegex(databaseIntegrityErrorsData);
        var databaseIntegrityErrors = new List<DatabaseIntegrityError>();

        foreach ((var entity, var errorMessage, var entitiesInConflict) in databaseIntegrityErrorsData)
        {
            var conflictEntities = entitiesInConflict.Select(e => new ConflictEntity((e is Alternative) ? TableNames.ALTERNATIVE : TableNames.CHACHAR, e));

            var databaseIntegrityError = entity is Alternative alternative
                ? new DatabaseIntegrityError(alternative, errorMessage, [.. conflictEntities])
                : new DatabaseIntegrityError((entity as Chachar)!, errorMessage, [.. conflictEntities]);

            databaseIntegrityErrors.Add(databaseIntegrityError);
        }

        var result = new DatabaseIntegrityErrorsContainer([.. databaseIntegrityErrors]).ToString();
        Assert.That(result, Does.Match(databaseIntegrityErrorsContainerRegex));
    }

    private static Regex GetDatabaseIntegrityErrorsContainerRegex(params (IEntity, string, IEnumerable<IEntity>)[] databaseIntegrityErrorsData)
    {
        var stringBuilder = new StringBuilder($@"^\{{""{JsonPropertyNames.DATABASE_INTEGRITY_ERRORS}"":\[");

        foreach ((var entity, var errorMessage, var entitiesInConflict) in databaseIntegrityErrorsData)
        {
            _ = stringBuilder.Append(
                CultureInfo.InvariantCulture,
                $@"(?=.*""{JsonPropertyNames.ERROR_MESSAGE}"":""{errorMessage}"",?)"
                    + $"(?=.*{GetEntityRegexPart(entity)},?)"
                    + $"(?=.*{GetConflictEntitiesRegexPart(entitiesInConflict)},?)"
            );
        }

        _ = stringBuilder.Append(@".*\]\}$");
        return new Regex(stringBuilder.ToString());
    }

    private static string GetConflictEntitiesRegexPart(IEnumerable<IEntity> entitiesInConflict)
    {
        var stringBuilder = new StringBuilder($@".*""{JsonPropertyNames.CONFLICT_ENTITIES}"":\[");

        foreach (var entity in entitiesInConflict)
        {
            _ = stringBuilder.Append(CultureInfo.InvariantCulture, $"(?=.*{GetEntityRegexPart(entity)},?)");
        }

        _ = stringBuilder.Append(@".*\]");
        return stringBuilder.ToString();
    }

    private static string GetEntityRegexPart(IEntity entity)
    {
        return entity is Alternative alternative
            ? @"\{"
                + $@"(?=.*""{JsonPropertyNames.ENTITY_TYPE}"":""{TableNames.ALTERNATIVE}"",?)"
                + $"(?=.*{GetAlternativeRegexPart(alternative)},?)"
                + @".*\}"
            : @"\{"
                + $@"(?=.*""{JsonPropertyNames.ENTITY_TYPE}"":""{TableNames.CHACHAR}"",?)"
                + $"(?=.*{GetChacharRegexPart((entity as Chachar)!)},?)"
                + @".*\}";
    }

    private static string GetChacharRegexPart(Chachar chachar) =>
        @$"""{JsonPropertyNames.ENTITY}"":\{{"
        + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""{chachar.TheCharacter}"",?)"
        + $@"(?=.*""{JsonPropertyNames.PINYIN}"":""{chachar.Pinyin}"",?)"
        + $@"(?=.*""{JsonPropertyNames.IPA}"":""{chachar.Ipa}"",?)"
        + $@"(?=.*""{JsonPropertyNames.TONE}"":{chachar.Tone},?)"
        + $@"(?=.*""{JsonPropertyNames.STROKES}"":{chachar.Strokes},?)"
        + $@"(?=.*""{JsonPropertyNames.RADICAL_CHARACTER}"":{((chachar.RadicalCharacter is { } radicalCharacter) ? $"\"{radicalCharacter}\"" : StringConstants.NULL_AS_STRING)},?)"
        + $@"(?=.*""{JsonPropertyNames.RADICAL_PINYIN}"":{((chachar.RadicalPinyin is { } radicalPinyin) ? $"\"{radicalPinyin}\"" : StringConstants.NULL_AS_STRING)},?)"
        + $@"(?=.*""{JsonPropertyNames.RADICAL_TONE}"":{((chachar.RadicalTone is { } radicalTone) ? radicalTone : StringConstants.NULL_AS_STRING)},?)"
        + $@"(?=.*""{JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER}"":{((chachar.RadicalAlternativeCharacter is { } radicalAlternativeCharacter) ? $"\"{radicalAlternativeCharacter}\"" : StringConstants.NULL_AS_STRING)},?)"
        + @".*\},?";

    private static string GetAlternativeRegexPart(Alternative alternative) =>
        @$"""{JsonPropertyNames.ENTITY}"":\{{"
        + $@"(?=.*""{JsonPropertyNames.THE_CHARACTER}"":""{alternative.TheCharacter}"",?)"
        + $@"(?=.*""{JsonPropertyNames.ORIGINAL_CHARACTER}"":""{alternative.OriginalCharacter}"",?)"
        + $@"(?=.*""{JsonPropertyNames.ORIGINAL_PINYIN}"":""{alternative.OriginalPinyin}"",?)"
        + $@"(?=.*""{JsonPropertyNames.ORIGINAL_TONE}"":{alternative.OriginalTone},?)"
        + $@"(?=.*""{JsonPropertyNames.STROKES}"":{alternative.Strokes},?)"
        + @".*\},?";
}
