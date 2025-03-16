using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Debug, "Triggering {dbAction} in the database")]
    public static partial void LogActionInDbDebug(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Information, "Triggering {dbAction} in the database")]
    public static partial void LogActionInDbInfo(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Error, "{dbAction} failed in the database")]
    public static partial void LogActionInDbFailedError(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Debug, "{dbAction} successfully performed in the database")]
    public static partial void LogActionInDbSuccessDebug(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Information, "{dbAction} successfully performed in the database")]
    public static partial void LogActionInDbSuccessInfo(ILogger logger, string dbAction);

    [LoggerMessage(LogLevel.Debug, "Body: {bodyString}")]
    public static partial void LogBodyDebug(ILogger logger, string bodyString);

    [LoggerMessage(LogLevel.Error, "Conflict {conflictEntityType}: {conflictEntity}")]
    public static partial void LogConflictError(ILogger logger, string conflictEntityType, IEntity conflictEntity);

    [LoggerMessage(LogLevel.Error, "Conflict {conflictEntitiesType}s: {conflictEntitiesString}")]
    public static partial void LogConflictsError(ILogger logger, string conflictEntitiesType, string conflictEntitiesString);

    [LoggerMessage(LogLevel.Information, "{entityType}: {entity}")]
    public static partial void LogEntityInfo(ILogger logger, string entityType, IEntity entity);

    [LoggerMessage(LogLevel.Information, "{entityType} created")]
    public static partial void LogEntityCreatedInfo(ILogger logger, string entityType);

    [LoggerMessage(LogLevel.Information, "{entityType} deleted")]
    public static partial void LogEntityDeletedInfo(ILogger logger, string entityType);

    [LoggerMessage(LogLevel.Error, "Expected Chachar or Alternative, got {receivedType}")]
    public static partial void LogEntityMismatchError(ILogger logger, string receivedType);

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

    [LoggerMessage(LogLevel.Debug, "Action initiated: {action} {entityType}")]
    public static partial void LogPostActionInitiatedDebug(ILogger logger, string action, string entityType);

    [LoggerMessage(LogLevel.Information, "Action initiated: {action} {entityType}")]
    public static partial void LogPostActionInitiatedInfo(ILogger logger, string action, string entityType);

    [LoggerMessage(LogLevel.Debug, "HTTP request received")]
    public static partial void LogRequestReceivedDebug(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "URL: {url}")]
    public static partial void LogUrlDebug(ILogger logger, string url);
}
