using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class EntityControllerCommons
{
    public static FieldErrorsContainer? GetPostInitialDataErrorsContainer<T>(
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldErrors = GetFieldErrors(
            entity,
            getFieldErrorMethods
        );

        return fieldErrors.Count > 0 ? new FieldErrorsContainer(fieldErrors) : null;
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

    public static FieldError? GetInvalidValueFieldError<T1, T2>(
        ILogger<T1> logger,
        T2 value,
        string columnName,
        Func<T2, string?> getErrorMessage
    ) where T1 : IEntityController
    {
        var errorMessage = getErrorMessage(value);

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(logger, value, columnName, errorMessage);
            return new FieldError(value, errorMessage, columnName);
        }

        return null;
    }

    public static FieldErrorsContainer? GetInvalidValueFieldErrorsContainer<T>(
        ILogger<T> logger,
        string errorMessage,
        params (object? errorValue, string fieldJsonPropertyName)[] valuesWithPropertyNames
    ) where T : IEntityController
    {
        if (valuesWithPropertyNames.Length == 0)
        {
            return null;
        }

        var fieldErrors = valuesWithPropertyNames.Select(
            pair =>
            {
                LogCommons.LogInvalidValueError(logger, pair.errorValue, pair.fieldJsonPropertyName, errorMessage);
                return new FieldError(pair.errorValue, errorMessage, pair.fieldJsonPropertyName);
            }
        );

        return new FieldErrorsContainer(fieldErrors);
    }

    private static List<FieldError> GetFieldErrors<T>(
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldErrors = new List<FieldError>();

        foreach (var getFieldError in getFieldErrorMethods)
        {
            if (getFieldError(entity) is { } error)
            {
                fieldErrors.Add(error);
            }
        }

        return fieldErrors;
    }
}
