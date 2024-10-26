using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants.Http;
using AsciiPinyin.Web.Server.Locals;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.NLOGCONFIG}")]
public partial class NLogConfigController(
    ILocals _locals,
    ILogger<NLogConfigController> _logger
) : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogGet(_logger, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissing(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, Errors.USER_AGENT_MISSING);
        }

        return StatusCode(
            StatusCodes.Status200OK,
            System.IO.File.ReadAllText(_locals.NLogConfigYamlPath)
        );
    }

    [LoggerMessage(LogLevel.Information, "GET nlog.config; User-Agent: {userAgent}")]
    public static partial void LogGet(ILogger logger, string userAgent);
}
