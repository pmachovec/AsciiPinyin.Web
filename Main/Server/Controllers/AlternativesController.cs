using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed class AlternativesController(AsciiPinyinContext _asciiPinyinContext) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Alternative> Get() => [.. _asciiPinyinContext.Alternatives];

    [HttpPost]
    public ActionResult<Alternative> Post(Alternative alternative)
    {
        Console.WriteLine($"Got alternative: {alternative}");
        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }
}
