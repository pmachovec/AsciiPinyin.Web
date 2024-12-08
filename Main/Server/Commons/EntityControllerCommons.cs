using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class EntityControllerCommons
{
    public static EntityFieldsErrorsContainer? GetPostInitialDataErrorsContainer<T>(
        string entityType,
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldsErrors = GetFieldsErrors(
            entity,
            getFieldErrorMethods
        );

        return fieldsErrors.Count > 0 ? new EntityFieldsErrorsContainer(entityType, [.. fieldsErrors]) : null;
    }

    public static string? GetCharacterErrorMessage(string? theCharacter)
    {
        string? errorMessage = null;

        if (theCharacter is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (theCharacter!.Length == 0)
        {
            errorMessage = Errors.EMPTY;
        }
        else if (TextUtils.GetStringRealLength(theCharacter!) > 1)
        {
            errorMessage = Errors.ONLY_ONE_CHARACTER_ALLOWED;
        }
        else if (!TextUtils.IsOnlyChineseCharacters(theCharacter!))
        {
            errorMessage = Errors.NO_SINGLE_CHINESE;
        }

        return errorMessage;
    }

    public static string? GetStrokesErrorMessage(byte? strokes)
    {
        string? errorMessage = null;

        if (strokes is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (strokes is < 1 or > 99)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ONE_TO_NINETY_NINE;
        }

        return errorMessage;
    }

    public static string? GetPinyinErrorMessage(string? pinyin)
    {
        string? errorMessage = null;

        if (pinyin is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (pinyin!.Length == 0)
        {
            errorMessage = Errors.EMPTY;
        }
        else if (!Regexes.AsciiLettersRegex().IsMatch(pinyin!))
        {
            errorMessage = Errors.NO_ASCII;
        }

        return errorMessage;
    }

    public static string? GetToneErrorMessage(byte? tone)
    {
        string? errorMessage = null;

        if (tone is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (tone > 4)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ZERO_TO_FOUR;
        }

        return errorMessage;
    }

    public static string GetEntityUnknownErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' does not identify an existing {entityType}";

    public static string GetEntityExistsErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies an already existing {entityType}";

    public static string GetNoRadicalErrorMessage(params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies a chachar, which is not radical";

    public static FieldError? GetInvalidValueFieldError<T1, T2>(
        ILogger<T1> logger,
        T2 value,
        string fieldName,
        Func<T2, string?> getErrorMessage
    ) where T1 : IEntityController
    {
        var errorMessage = getErrorMessage(value);

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(logger, value, fieldName, errorMessage);
            return new FieldError(fieldName, value, errorMessage);
        }

        return null;
    }

    private static List<FieldError> GetFieldsErrors<T>(
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldsErrors = new List<FieldError>();

        foreach (var getFieldError in getFieldErrorMethods)
        {
            if (getFieldError(entity) is { } error)
            {
                fieldsErrors.Add(error);
            }
        }

        return fieldsErrors;
    }
}
