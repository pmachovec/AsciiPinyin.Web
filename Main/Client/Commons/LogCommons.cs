using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Error, $"The used combination '{IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT} + {IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT} + {IDs.CHACHAR_FORM_TONE_INPUT}' identifies an alternative that already exists in the database.")]
    public static partial void LogAlternativeAlreadyExistsError(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Result of '{method} {entitiesApiName}' is null.")]
    public static partial void LogApiNullError(ILogger logger, HttpMethod method, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is empty.")]
    public static partial void LogApiEmptyError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Error occured on the server side when retrieving '{entitiesApiName}'.")]
    public static partial void LogApiServerSideError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, $"The used combination '{IDs.CHACHAR_FORM_THE_CHARACTER_INPUT} + {IDs.CHACHAR_FORM_PINYIN_INPUT} + {IDs.CHACHAR_FORM_TONE_INPUT}' identifies a chachar that already exists in the database.")]
    public static partial void LogChacharAlreadyExistsError(ILogger logger);

    [LoggerMessage(LogLevel.Information, "CREATE {entityClassName}: {entity}")]
    public static partial void LogCreateInfo(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Debug, "Database integrity verification")]
    public static partial void LogDatabaseIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Information, "DELETE {entityClassName}: {entity}")]
    public static partial void LogDeleteInfo(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Error)]
    public static partial void LogExceptionError(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failure; status code: {statusCode}, message: {message}")]
    public static partial void LogFailureError(ILogger logger, HttpStatusCode statusCode, string message);

    [LoggerMessage(LogLevel.Information, "Form data: {entity}")]
    public static partial void LogFormDataInfo(ILogger logger, IEntity entity);

    [LoggerMessage(LogLevel.Information, "HTTP method invoked: {method}; action: {action}")]
    public static partial void LogHttpMethodInfo(ILogger logger, HttpMethod method, string action);

    [LoggerMessage(LogLevel.Error, "HTTP method {method} processing failed")]
    public static partial void LogHttpMethodFailedError(ILogger logger, HttpMethod method);

    [LoggerMessage(LogLevel.Information, "HTTP method {method} processed successfully")]
    public static partial void LogHttpMethodSuccessInfo(ILogger logger, HttpMethod method);

    [LoggerMessage(LogLevel.Debug, "Initial form integrity verification")]
    public static partial void LogInitialIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Invalid form value; value: {errorValue}, input ID: {inputId}, error: {errorMessage}")]
    public static partial void LogInvalidFormValueError(ILogger logger, object? errorValue, string inputId, string errorMessage);

    [LoggerMessage(LogLevel.Information, "{method}: {entity}")]
    public static partial void LogMethodEntityDebug(ILogger logger, HttpMethod method, IEntity entity);

    [LoggerMessage(LogLevel.Information, "Success")]
    public static partial void LogSuccessInfo(ILogger logger);
}
