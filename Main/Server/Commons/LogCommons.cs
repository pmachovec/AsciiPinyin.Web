using AsciiPinyin.Web.Server.Constants.Http;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Information, "GET all {entitiesName}; User-Agent: {userAgent}")]
    public static partial void LogGetAllEntities(ILogger logger, string entitiesName, string userAgent);

    [LoggerMessage(LogLevel.Information, "POST: {entity}; User-Agent: {userAgent}")]
    public static partial void LogPostEntity(ILogger logger, IEntity entity, string userAgent);

    [LoggerMessage(LogLevel.Error, Errors.USER_AGENT_MISSING)]
    public static partial void LogUserAgentMissing(ILogger logger);
}
