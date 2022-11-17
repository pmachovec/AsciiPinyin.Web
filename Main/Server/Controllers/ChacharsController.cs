using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route("/characters")]
public sealed class ChacharsController : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; }

    public ChacharsController(AsciiPinyinContext asciiPinyinContext)
    {
        AsciiPinyinContext = asciiPinyinContext;
    }

    [HttpGet]
    public IEnumerable<Chachar> Get()
    {
        return AsciiPinyinContext.Chachars.ToArray();
    }
}
