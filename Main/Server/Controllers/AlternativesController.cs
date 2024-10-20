using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed class AlternativesController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<AlternativesController> _logger
) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Alternative>> Get()
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogGetAllEntities(_logger, "alternatives", userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissing(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, StringConstants.USER_AGENT_MISSING);
        }

        return StatusCode(StatusCodes.Status200OK, _asciiPinyinContext.Alternatives);
    }

    [HttpPost]
    public ActionResult<Alternative> Post(Alternative alternative)
    {
        if (Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogPostEntity(_logger, alternative, userAgent!);
        }
        else
        {
            LogCommons.LogUserAgentMissing(_logger);
            return StatusCode(StatusCodes.Status400BadRequest, StringConstants.USER_AGENT_MISSING);
        }

        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }
}
