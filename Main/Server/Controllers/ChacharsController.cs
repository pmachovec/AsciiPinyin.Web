namespace AsciiPinyin.Web.Server.Controllers;

using AsciiPinyin.Web.Shared.Models;
using Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/characters")]
public class ChacharsController : ControllerBase
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
