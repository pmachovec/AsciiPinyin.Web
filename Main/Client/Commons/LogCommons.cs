using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is null.")]
    public static partial void LogApiNullError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Result of retrieving '{entitiesApiName}' is empty.")]
    public static partial void LogApiEmptyError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Error, "Error occured on the server side when retrieving '{entitiesApiName}'.")]
    public static partial void LogApiServerSideError(ILogger logger, string entitiesApiName);

    [LoggerMessage(LogLevel.Information, "CREATE {entityClassName}: {entity}")]
    public static partial void LogCreateInfo(ILogger logger, Type entityClassName, IEntity entity);

    [LoggerMessage(LogLevel.Debug, "Database integrity verification")]
    public static partial void LogDatabaseIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Error, "{message}")]
    public static partial void LogError(ILogger logger, string message);

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
