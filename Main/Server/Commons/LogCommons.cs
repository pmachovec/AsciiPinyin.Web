using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Error, "{message}")]
    public static partial void LogError(ILogger logger, string message);

    [LoggerMessage(LogLevel.Information, "GET all {entitiesName}; User-Agent: {userAgent}")]
    public static partial void LogGetAllEntitiesInfo(ILogger logger, string entitiesName, string userAgent);

    [LoggerMessage(LogLevel.Debug, "Database radicals integrity verification")]
    public static partial void LogDatabaseRadicalIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Initial data integrity verification")]
    public static partial void LogInitialIntegrityVerificationDebug(ILogger logger);

    [LoggerMessage(LogLevel.Information, "POST: {entity}; User-Agent: {userAgent}")]
    public static partial void LogPostEntityInfo(ILogger logger, IEntity entity, string userAgent);

    [LoggerMessage(LogLevel.Error, Errors.USER_AGENT_MISSING)]
    public static partial void LogUserAgentMissingError(ILogger logger);
}
