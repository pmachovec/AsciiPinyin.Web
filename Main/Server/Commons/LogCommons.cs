using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Information, "Triggering {dbAction} in the database; action: {action}")]
    public static partial void LogActionInDbInfo(ILogger logger, string dbAction, string action);

    [LoggerMessage(LogLevel.Error, "{dbAction} failed in the database")]
    public static partial void LogActionInDbFailedError(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Information, "{dbAction} successfully performed in the database")]
    public static partial void LogActionInDbSuccessInfo(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Debug, "Body: {bodyString}")]
    public static partial void LogBodyDebug(ILogger logger, string bodyString);

    [LoggerMessage(LogLevel.Error, "{databaseIntegrityErrorsContainer}")]
    public static partial void LogDatabaseIntegrityErrorsContainerError(ILogger logger, DatabaseIntegrityErrorsContainer databaseIntegrityErrorsContainer);

    [LoggerMessage(LogLevel.Debug, "Database integrity verification")]
    public static partial void LogDatabaseIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Error, "{errorMessage}; {entityType}: {entity}")]
    public static partial void LogEntityError(ILogger logger, string errorMessage, string entityType, IEntity entity);

    [LoggerMessage(LogLevel.Error, "{errorMessage}; {entityType}: {entity}; {additionalInfo}")]
    public static partial void LogEntityError(ILogger logger, string errorMessage, string entityType, IEntity entity, string additionalInfo);

    [LoggerMessage(LogLevel.Information, "Entity type: {entityType}; entity: {entity}")]
    public static partial void LogEntityInfo(ILogger logger, string entityType, IEntity entity);

    [LoggerMessage(LogLevel.Error, "{message}")]
    public static partial void LogError(ILogger logger, string message);

    [LoggerMessage(LogLevel.Debug, "Headers: [{headersString}]")]
    public static partial void LogHeadersDebug(ILogger logger, string headersString);

    [LoggerMessage(LogLevel.Debug, "Integrity verification successful")]
    public static partial void LogIntegrityVerificationSuccessDebug(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Invalid value; value: {errorValue}, property: {fieldJsonPropertyName}, error: {errorMessage}")]
    public static partial void LogInvalidValueError(ILogger logger, object? errorValue, string fieldJsonPropertyName, string errorMessage);

    [LoggerMessage(LogLevel.Debug, "Method: {method}")]
    public static partial void LogMethodDebug(ILogger logger, string method);

    [LoggerMessage(LogLevel.Error, "Field error - {fieldName}: [{valuesString}]")]
    public static partial void LogFieldErrors(ILogger logger, string fieldName, string valuesString);

    [LoggerMessage(LogLevel.Debug, "HTTP request received")]
    public static partial void LogRequestReceivedDebug(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "URL: {url}")]
    public static partial void LogUrlDebug(ILogger logger, string url);
}
