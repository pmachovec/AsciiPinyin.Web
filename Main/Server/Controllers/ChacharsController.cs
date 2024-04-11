using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route("/asciipinyin/characters")]
public sealed class ChacharsController(AsciiPinyinContext asciiPinyinContext) : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; } = asciiPinyinContext;

    [HttpGet]
    public IEnumerable<Chachar> Get() => [.. AsciiPinyinContext.Chachars];
}
