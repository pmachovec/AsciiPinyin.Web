using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed partial class AlternativesController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<AlternativesController> _logger
) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Alternative> Get()
    {
        LogGet(_logger);
        return [.. _asciiPinyinContext.Alternatives];
    }

    [HttpPost]
    public ActionResult<Alternative> Post(Alternative alternative)
    {
        LogPost(_logger, alternative);
        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }

    [LoggerMessage(LogLevel.Information, "GET all alternatives")]
    private static partial void LogGet(ILogger logger);

    [LoggerMessage(LogLevel.Information, "POST: {alternative}")]
    private static partial void LogPost(ILogger logger, Alternative alternative);
}
