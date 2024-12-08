using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Test.Constants;
using NUnit.Framework;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Shared.Test.DTO;

[TestFixture]
internal sealed class EntityFieldsErrorsContainerTest
{
    private const string TEST_ERROR_MESSAGE_1 = "error message 1";
    private const string TEST_ERROR_MESSAGE_2 = "error message 2";
    private const string TEST_ERROR_MESSAGE_3 = "error message 3";
    private const string TEST_ERROR_MESSAGE_4 = "error message 4";
    private const string TEST_ERROR_MESSAGE_5 = "error message 5";
    private const string TEST_ERROR_MESSAGE_6 = "error message 6";
    private const string TEST_ERROR_MESSAGE_7 = "error message 7";
    private const string TEST_ERROR_MESSAGE_8 = "error message 8";

    private const string TEST_FIELD_1 = "test_field_1";
    private const string TEST_FIELD_2 = "test_field_2";
    private const string TEST_FIELD_3 = "test_field_3";
    private const string TEST_FIELD_4 = "test_field_4";
    private const string TEST_FIELD_5 = "test_field_5";
    private const string TEST_FIELD_6 = "test_field_6";
    private const string TEST_FIELD_7 = "test_field_7";
    private const string TEST_FIELD_8 = "test_field_8";

    private const string STRING = "test string";
    private const sbyte SBYTE_POSITIVE = 1;
    private const sbyte SBYTE_NEGATIVE = -1;
    private const sbyte SBYTE_ZERO = 0;
    private const float FLOAT_POSITIVE = 1.1f;
    private const float FLOAT_NEGATIVE = -1.1f;
    private const float FLOAT_ZERO = 0.0f;
    private const object? NULL = null;

    private const string STRING_AS_STRING = $@"""{STRING}""";
    private const string SBYTE_POSITIVE_AS_STRING = "1";
    private const string SBYTE_NEGATIVE_AS_STRING = "-1";
    private const string SBYTE_ZERO_AS_STRING = "0";
    private const string FLOAT_POSITIVE_AS_STRING = "1.1";
    private const string FLOAT_NEGATIVE_AS_STRING = "-1.1";
    private const string FLOAT_ZERO_AS_STRING = "0";

    [Test]
    public void EmptyContainerToStringTest()
    {
        var fieldsErrorsContainer = new EntityFieldsErrorsContainer();
        var result = fieldsErrorsContainer.ToString();

        Assert.That(result, Is.EqualTo($@"{{""{JsonPropertyNames.ENTITY_FIELDS_ERRORS}"":[]}}"));
    }

    [TestCase(STRING, STRING_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - string")]
    [TestCase(SBYTE_POSITIVE, SBYTE_POSITIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - positive integer")]
    [TestCase(SBYTE_NEGATIVE, SBYTE_NEGATIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - negative integer")]
    [TestCase(SBYTE_ZERO, SBYTE_ZERO_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - zero integer")]
    [TestCase(FLOAT_POSITIVE, FLOAT_POSITIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - positive float")]
    [TestCase(FLOAT_NEGATIVE, FLOAT_NEGATIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - negative float")]
    [TestCase(FLOAT_ZERO, FLOAT_ZERO_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - zero float")]
    [TestCase(NULL, StringConstants.NULL_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleChacharSingleFieldErrorToStringTest)} - null")]
    public void SingleChacharSingleFieldErrorToStringTest(object? value, string valueString)
    {
        var fieldsErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            TableNames.CHACHAR,
            TEST_FIELD_1,
            valueString,
            TEST_ERROR_MESSAGE_1
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(
            TableNames.CHACHAR,
            new FieldError(
                TEST_FIELD_1,
                value,
                TEST_ERROR_MESSAGE_1
            )
        );

        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldsErrorsContainerRegex));
    }

    [TestCase(STRING, STRING_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - string")]
    [TestCase(SBYTE_POSITIVE, SBYTE_POSITIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - positive integer")]
    [TestCase(SBYTE_NEGATIVE, SBYTE_NEGATIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - negative integer")]
    [TestCase(SBYTE_ZERO, SBYTE_ZERO_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - zero integer")]
    [TestCase(FLOAT_POSITIVE, FLOAT_POSITIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - positive float")]
    [TestCase(FLOAT_NEGATIVE, FLOAT_NEGATIVE_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - negative float")]
    [TestCase(FLOAT_ZERO, FLOAT_ZERO_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - zero float")]
    [TestCase(NULL, StringConstants.NULL_AS_STRING, TestName = $"{nameof(EntityFieldsErrorsContainerTest)}.{nameof(SingleAlternativeSingleFieldErrorToStringTest)} - null")]
    public void SingleAlternativeSingleFieldErrorToStringTest(object? value, string valueString)
    {
        var fieldsErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            TableNames.ALTERNATIVE,
            TEST_FIELD_1,
            valueString,
            TEST_ERROR_MESSAGE_1
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(
            TableNames.ALTERNATIVE,
            new FieldError(
                TEST_FIELD_1,
                value,
                TEST_ERROR_MESSAGE_1
            )
        );

        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldsErrorsContainerRegex));
    }

    [Test]
    public void SingleChacharMultipleFieldErrorsToStringTest() => SingleEntityMultipleFieldErrorsToStringTest(TableNames.CHACHAR);

    [Test]
    public void SingleAlternativeMultipleFieldErrorsToStringTest() => SingleEntityMultipleFieldErrorsToStringTest(TableNames.ALTERNATIVE);

    [Test]
    public void SingleChacharSingleAlternativeMultipleFieldErrorsToStringTest()
    {
        var fieldErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            (
                TableNames.CHACHAR,
                [
                    (TEST_FIELD_1, STRING_AS_STRING, TEST_ERROR_MESSAGE_1),
                    (TEST_FIELD_2, SBYTE_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_2),
                    (TEST_FIELD_3, SBYTE_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_3),
                    (TEST_FIELD_4, SBYTE_ZERO_AS_STRING, TEST_ERROR_MESSAGE_4)
                ]
            ),
            (
                TableNames.ALTERNATIVE,
                [
                    (TEST_FIELD_5, FLOAT_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_5),
                    (TEST_FIELD_6, FLOAT_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_6),
                    (TEST_FIELD_7, FLOAT_ZERO_AS_STRING, TEST_ERROR_MESSAGE_7),
                    (TEST_FIELD_8, StringConstants.NULL_AS_STRING, TEST_ERROR_MESSAGE_8)
                ]
            )
        );

        var fieldsError1 = new EntityFieldsError(
            TableNames.CHACHAR,
            new FieldError(TEST_FIELD_1, STRING, TEST_ERROR_MESSAGE_1),
            new FieldError(TEST_FIELD_2, SBYTE_POSITIVE, TEST_ERROR_MESSAGE_2),
            new FieldError(TEST_FIELD_3, SBYTE_NEGATIVE, TEST_ERROR_MESSAGE_3),
            new FieldError(TEST_FIELD_4, SBYTE_ZERO, TEST_ERROR_MESSAGE_4)
        );

        var fieldsError2 = new EntityFieldsError(
            TableNames.ALTERNATIVE,
            new FieldError(TEST_FIELD_5, FLOAT_POSITIVE, TEST_ERROR_MESSAGE_5),
            new FieldError(TEST_FIELD_6, FLOAT_NEGATIVE, TEST_ERROR_MESSAGE_6),
            new FieldError(TEST_FIELD_7, FLOAT_ZERO, TEST_ERROR_MESSAGE_7),
            new FieldError(TEST_FIELD_8, NULL, TEST_ERROR_MESSAGE_8)
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(fieldsError1, fieldsError2);
        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldErrorsContainerRegex));
    }

    [Test]
    public void MultipleChacharsSingleFieldErrorsToStringTest() => MultipleEntitiesOfSameTypeSingleFieldErrorsToStringTest(TableNames.CHACHAR);

    [Test]
    public void MultipleAlternativesSingleFieldErrorsToStringTest() => MultipleEntitiesOfSameTypeSingleFieldErrorsToStringTest(TableNames.ALTERNATIVE);

    [Test]
    public void MultipleChacharsMultipleFieldErrorsToStringTest() => MultipleEntitiesOfSameTypeMultipleFieldErrorsToStringTest(TableNames.CHACHAR);

    [Test]
    public void MultipleAlternativesMultipleFieldErrorsToStringTest() => MultipleEntitiesOfSameTypeMultipleFieldErrorsToStringTest(TableNames.ALTERNATIVE);

    [Test]
    public void MultipleChacharsMultipleAlternativesSingleFieldErrorsToStringTest()
    {
        var fieldErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            (TableNames.CHACHAR, [(TEST_FIELD_1, STRING_AS_STRING, TEST_ERROR_MESSAGE_1)]),
            (TableNames.CHACHAR, [(TEST_FIELD_2, SBYTE_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_2)]),
            (TableNames.CHACHAR, [(TEST_FIELD_3, SBYTE_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_3)]),
            (TableNames.CHACHAR, [(TEST_FIELD_4, SBYTE_ZERO_AS_STRING, TEST_ERROR_MESSAGE_4)]),
            (TableNames.ALTERNATIVE, [(TEST_FIELD_5, FLOAT_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_5)]),
            (TableNames.ALTERNATIVE, [(TEST_FIELD_6, FLOAT_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_6)]),
            (TableNames.ALTERNATIVE, [(TEST_FIELD_7, FLOAT_ZERO_AS_STRING, TEST_ERROR_MESSAGE_7)]),
            (TableNames.ALTERNATIVE, [(TEST_FIELD_8, StringConstants.NULL_AS_STRING, TEST_ERROR_MESSAGE_8)])
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(
            new EntityFieldsError(TableNames.CHACHAR, new FieldError(TEST_FIELD_1, STRING, TEST_ERROR_MESSAGE_1)),
            new EntityFieldsError(TableNames.CHACHAR, new FieldError(TEST_FIELD_2, SBYTE_POSITIVE, TEST_ERROR_MESSAGE_2)),
            new EntityFieldsError(TableNames.CHACHAR, new FieldError(TEST_FIELD_3, SBYTE_NEGATIVE, TEST_ERROR_MESSAGE_3)),
            new EntityFieldsError(TableNames.CHACHAR, new FieldError(TEST_FIELD_4, SBYTE_ZERO, TEST_ERROR_MESSAGE_4)),
            new EntityFieldsError(TableNames.ALTERNATIVE, new FieldError(TEST_FIELD_5, FLOAT_POSITIVE, TEST_ERROR_MESSAGE_5)),
            new EntityFieldsError(TableNames.ALTERNATIVE, new FieldError(TEST_FIELD_6, FLOAT_NEGATIVE, TEST_ERROR_MESSAGE_6)),
            new EntityFieldsError(TableNames.ALTERNATIVE, new FieldError(TEST_FIELD_7, FLOAT_ZERO, TEST_ERROR_MESSAGE_7)),
            new EntityFieldsError(TableNames.ALTERNATIVE, new FieldError(TEST_FIELD_8, NULL, TEST_ERROR_MESSAGE_8))
        );

        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldErrorsContainerRegex));
    }

    private static void SingleEntityMultipleFieldErrorsToStringTest(string entityType)
    {
        var fieldErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            entityType,
            (TEST_FIELD_1, STRING_AS_STRING, TEST_ERROR_MESSAGE_1),
            (TEST_FIELD_2, SBYTE_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_2),
            (TEST_FIELD_3, SBYTE_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_3)
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(
            entityType,
            new FieldError(TEST_FIELD_1, STRING, TEST_ERROR_MESSAGE_1),
            new FieldError(TEST_FIELD_2, SBYTE_POSITIVE, TEST_ERROR_MESSAGE_2),
            new FieldError(TEST_FIELD_3, SBYTE_NEGATIVE, TEST_ERROR_MESSAGE_3)
        );

        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldErrorsContainerRegex));
    }

    private static void MultipleEntitiesOfSameTypeSingleFieldErrorsToStringTest(string entityType)
    {
        var fieldErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            (entityType, [(TEST_FIELD_1, STRING_AS_STRING, TEST_ERROR_MESSAGE_1)]),
            (entityType, [(TEST_FIELD_2, SBYTE_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_2)]),
            (entityType, [(TEST_FIELD_3, SBYTE_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_3)])
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(
            new EntityFieldsError(entityType, new FieldError(TEST_FIELD_1, STRING, TEST_ERROR_MESSAGE_1)),
            new EntityFieldsError(entityType, new FieldError(TEST_FIELD_2, SBYTE_POSITIVE, TEST_ERROR_MESSAGE_2)),
            new EntityFieldsError(entityType, new FieldError(TEST_FIELD_3, SBYTE_NEGATIVE, TEST_ERROR_MESSAGE_3))
        );

        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldErrorsContainerRegex));
    }

    private static void MultipleEntitiesOfSameTypeMultipleFieldErrorsToStringTest(string entityType)
    {
        var fieldErrorsContainerRegex = GetFieldsErrorsContainerRegex(
            (
                entityType,
                [
                    (TEST_FIELD_1, STRING_AS_STRING, TEST_ERROR_MESSAGE_1),
                    (TEST_FIELD_2, SBYTE_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_2),
                    (TEST_FIELD_3, SBYTE_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_3)
                ]
            ),
            (
                entityType,
                [
                    (TEST_FIELD_5, FLOAT_POSITIVE_AS_STRING, TEST_ERROR_MESSAGE_4),
                    (TEST_FIELD_6, FLOAT_NEGATIVE_AS_STRING, TEST_ERROR_MESSAGE_5),
                    (TEST_FIELD_7, FLOAT_ZERO_AS_STRING, TEST_ERROR_MESSAGE_6)
                ]
            )
        );

        var fieldsError1 = new EntityFieldsError(
            entityType,
            new FieldError(TEST_FIELD_1, STRING, TEST_ERROR_MESSAGE_1),
            new FieldError(TEST_FIELD_2, SBYTE_POSITIVE, TEST_ERROR_MESSAGE_2),
            new FieldError(TEST_FIELD_3, SBYTE_NEGATIVE, TEST_ERROR_MESSAGE_3)
        );

        var fieldsError2 = new EntityFieldsError(
            entityType,
            new FieldError(TEST_FIELD_5, FLOAT_POSITIVE, TEST_ERROR_MESSAGE_4),
            new FieldError(TEST_FIELD_6, FLOAT_NEGATIVE, TEST_ERROR_MESSAGE_5),
            new FieldError(TEST_FIELD_7, FLOAT_ZERO, TEST_ERROR_MESSAGE_6)
        );

        var fieldsErrorsContainer = new EntityFieldsErrorsContainer(fieldsError1, fieldsError2);
        var result = fieldsErrorsContainer.ToString();
        Assert.That(result, Does.Match(fieldErrorsContainerRegex));
    }

    private static Regex GetFieldsErrorsContainerRegex(
        string entityType,
        string fieldName,
        string fieldValueString,
        string errorMessage
    ) => GetFieldsErrorsContainerRegex(entityType, [(fieldName, fieldValueString, errorMessage)]);

    private static Regex GetFieldsErrorsContainerRegex(
        string entityType,
        params (string, string, string)[] fieldErrorsData
    ) => GetFieldsErrorsContainerRegex([(entityType, fieldErrorsData)]);

    private static Regex GetFieldsErrorsContainerRegex(
        params (string, IEnumerable<(string, string, string)>)[] entityFieldsErrorsData
    ) => new($@"^\{{""{JsonPropertyNames.ENTITY_FIELDS_ERRORS}"":\[{GetEntityFieldsErrorsDataRegexPart(entityFieldsErrorsData)}.*\]\}}$");


    private static string GetEntityFieldsErrorsDataRegexPart(IEnumerable<(string, IEnumerable<(string, string, string)>)> entityFieldsErrorsData)
    {
        var stringBuilder = new StringBuilder();

        foreach ((var entityType, var fieldErrorsData) in entityFieldsErrorsData)
        {
            _ = stringBuilder.Append(
                CultureInfo.InvariantCulture,
                $"(?=.*{GetSingleEntityFieldsErrorDataRegex(entityType, fieldErrorsData)},?)"
            );
        }

        return stringBuilder.ToString();
    }

    private static string GetSingleEntityFieldsErrorDataRegex(string entityType, IEnumerable<(string, string, string)> fieldErrorsData) =>
        @"\{"
        + $@"(?=.*""{JsonPropertyNames.ENTITY_TYPE}"":""{entityType}"",?)"
        + $"(?=.*{GetFieldErrorsDataRegexPart(fieldErrorsData)},?)"
        + @".*\},?";

    private static string GetFieldErrorsDataRegexPart(IEnumerable<(string, string, string)> fieldErrorsData)
    {
        var stringBuilder = new StringBuilder($@"""{JsonPropertyNames.FIELD_ERRORS}"":\[");

        foreach ((var fieldName, var fieldValueString, var errorMessage) in fieldErrorsData)
        {
            _ = stringBuilder.Append(
                CultureInfo.InvariantCulture,
                $@"(?=.*{GetSingleFieldErrorDataRegexPart(fieldName, fieldValueString, errorMessage)},?)"
            );
        }

        _ = stringBuilder.Append(@".*\]");
        return stringBuilder.ToString();
    }

    private static string GetSingleFieldErrorDataRegexPart(string fieldName, string fieldValueString, string errorMessage) =>
        @"\{"
        + $@"(?=.*""{JsonPropertyNames.FIELD_NAME}"":""{fieldName}"",?)"
        + $@"(?=.*""{JsonPropertyNames.FIELD_VALUE}"":{fieldValueString},?)"
        + $@"(?=.*""{JsonPropertyNames.ERROR_MESSAGE}"":""{errorMessage}"",?)"
        + @".*\},?";
}
