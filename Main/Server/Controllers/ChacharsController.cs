using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed partial class ChacharsController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<ChacharsController> _logger
) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Chachar> Get()
    {
        LogGet(_logger);
        return [.. _asciiPinyinContext.Chachars];
    }

    [HttpPost]
    public ActionResult<Chachar> Post(Chachar chachar)
    {
        LogPost(_logger, chachar);
        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }

    [LoggerMessage(1, LogLevel.Information, "GET all chachars")]
    private static partial void LogGet(ILogger logger);

    [LoggerMessage(1, LogLevel.Information, "POST: {chachar}")]
    private static partial void LogPost(ILogger logger, Chachar chachar);
}
