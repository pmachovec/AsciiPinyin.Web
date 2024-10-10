using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed class ChacharsController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<ChacharsController> _logger
) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Chachar>> Get()
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogGetAllEntities(_logger, "characters", userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissing(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, StringConstants.USER_AGENT_MISSING);
        }

        return StatusCode(StatusCodes.Status200OK, _asciiPinyinContext.Chachars);
    }

    [HttpPost]
    public ActionResult<Chachar> Post(Chachar chachar)
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogPostEntity(_logger, chachar, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissing(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, StringConstants.USER_AGENT_MISSING);
        }

        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }
}
