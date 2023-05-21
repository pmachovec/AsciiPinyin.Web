using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route("/asciipinyin/alternatives")]
public sealed class AlternativesController : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; }

    public AlternativesController(AsciiPinyinContext asciiPinyinContext) => AsciiPinyinContext = asciiPinyinContext;

    [HttpGet]
    public IEnumerable<Alternative> Get() => AsciiPinyinContext.Alternatives.ToArray();
}
