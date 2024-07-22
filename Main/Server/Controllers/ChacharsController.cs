using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed class ChacharsController(AsciiPinyinContext asciiPinyinContext) : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; } = asciiPinyinContext;

    [HttpGet]
    public IEnumerable<Chachar> Get() => [.. AsciiPinyinContext.Chachars];

    [HttpPost]
    public ActionResult<Chachar> Post(Chachar chachar)
    {
        Console.WriteLine($"Got chachar: {chachar}");
        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }
}
