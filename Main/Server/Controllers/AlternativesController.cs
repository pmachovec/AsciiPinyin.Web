using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route("/asciipinyin/alternatives")]
public sealed class AlternativesController : ControllerBase
{
    private AsciiPinyinContext AsciiPinyinContext { get; }

    public AlternativesController(AsciiPinyinContext asciiPinyinContext)
    {
        AsciiPinyinContext = asciiPinyinContext;
    }

    [HttpGet]
    public IEnumerable<Alternative> Get()
    {
        return AsciiPinyinContext.Alternatives.ToArray();
    }
}
