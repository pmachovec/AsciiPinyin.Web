using AsciiPinyin.Web.Server.Constants;

namespace AsciiPinyin.Web.Server.Commons;

internal static partial class LogCommons
{
    [LoggerMessage(LogLevel.Error, "{message}")]
    public static partial void LogError(ILogger logger, string message);

    [LoggerMessage(LogLevel.Error, Errors.USER_AGENT_MISSING)]
    public static partial void LogUserAgentMissingError(ILogger logger);
}
