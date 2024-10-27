using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class EntityControllerCommons
{
    public static object? GetGetErrorWithLogging(
        string apiName,
        IHeaderDictionary requestHeaders,
        ILogger logger
    )
    {
        if (requestHeaders.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogGetAllEntitiesInfo(logger, apiName, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissingError(logger);
            return Errors.USER_AGENT_MISSING;
        }

        return null;
    }

    public static object? GetPostErrorWithLogging<T>(
        T entity,
        IHeaderDictionary requestHeaders,
        ILogger logger,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        if (requestHeaders.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogPostEntityInfo(logger, entity, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissingError(logger);
            return Errors.USER_AGENT_MISSING;
        }

        LogIntegrityVerificationDebug(logger);
        var fieldErrors = GetFieldErrors(
            entity,
            getFieldErrorMethods
        );

        if (fieldErrors.Count > 0)
        {
            var fieldErrorsContainer = new FieldErrorsContainer(fieldErrors);
            LogCommons.LogError(logger, fieldErrorsContainer.ToString());
            return fieldErrorsContainer;
        }

        return null;
    }

    public static FieldError? GetTheCharacterError(IEntity entity)
    {
        var errorMessage = GetCharacterErrorMessage(entity.TheCharacter);
        return errorMessage is not null ? new FieldError(entity.TheCharacter, errorMessage, ColumnNames.THE_CHARACTER) : null;
    }

    public static FieldError? GetStrokesError(IEntity entity)
    {
        string? errorMessage = null;

        if (entity.Strokes is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (entity.Strokes is < 1 or > 99)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ONE_TO_NINETY_NINE;
        }

        return errorMessage is not null ? new FieldError(entity.Strokes, errorMessage, ColumnNames.STROKES) : null;
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

    [LoggerMessage(LogLevel.Information, "GET all {entitiesName}; User-Agent: {userAgent}")]
    private static partial void LogGetAllEntitiesInfo(ILogger logger, string entitiesName, string userAgent);

    [LoggerMessage(LogLevel.Information, "POST: {entity}; User-Agent: {userAgent}")]
    private static partial void LogPostEntityInfo(ILogger logger, IEntity entity, string userAgent);

    [LoggerMessage(LogLevel.Debug, "Integrity verification")]
    private static partial void LogIntegrityVerificationDebug(ILogger logger);
}
