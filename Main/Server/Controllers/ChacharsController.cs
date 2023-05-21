using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route("/asciipinyin/characters")]
public sealed class ChacharsController : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; }

    public ChacharsController(AsciiPinyinContext asciiPinyinContext) => AsciiPinyinContext = asciiPinyinContext;

    [HttpGet]
    public IEnumerable<Chachar> Get() => AsciiPinyinContext.Chachars.ToArray();
}
