using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Locals;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.NLOGCONFIG}")]
public sealed class NLogConfigController(
    ILocals _locals,
    ILogger<NLogConfigController> _logger
) : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogGetInfo(_logger, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, Errors.USER_AGENT_MISSING);
        }

        return StatusCode(
            StatusCodes.Status200OK,
            System.IO.File.ReadAllText(_locals.NLogConfigYamlPath)
        );
    }
}
