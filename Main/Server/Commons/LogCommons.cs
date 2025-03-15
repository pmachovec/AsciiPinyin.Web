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

    [LoggerMessage(LogLevel.Error, "{errorMessage}; {entityType}: {entity}")]
    public static partial void LogEntityError(ILogger logger, string errorMessage, string entityType, IEntity entity);

    [LoggerMessage(LogLevel.Error, "{errorMessage}; {entityType}: {entity}; {additionalInfo}")]
    public static partial void LogEntityError(ILogger logger, string errorMessage, string entityType, IEntity entity, string additionalInfo);

    [LoggerMessage(LogLevel.Error, "Expected {entityType}, got {receivedType}")]
    public static partial void LogEntityMismatchError(ILogger logger, string entityType, string receivedType);

    [LoggerMessage(LogLevel.Error, "Expected {entityType}, got null")]
    public static partial void LogEntityNullError(ILogger logger, string entityType);

    [LoggerMessage(LogLevel.Error, "{message}")]
    public static partial void LogError(ILogger logger, string message);

    [LoggerMessage(LogLevel.Debug, "Headers: [{headersString}]")]
    public static partial void LogHeadersDebug(ILogger logger, string headersString);

    [LoggerMessage(LogLevel.Debug, "Method: {method}")]
    public static partial void LogMethodDebug(ILogger logger, string method);

    [LoggerMessage(LogLevel.Error, "Field error - {fieldName}: [{valuesString}]")]
    public static partial void LogFieldErrors(ILogger logger, string fieldName, string valuesString);

    [LoggerMessage(LogLevel.Debug, "HTTP request received")]
    public static partial void LogRequestReceivedDebug(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "URL: {url}")]
    public static partial void LogUrlDebug(ILogger logger, string url);
}
