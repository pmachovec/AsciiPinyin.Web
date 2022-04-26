namespace AsciiPinyin.Web.Controllers;

using Data;
using Microsoft.AspNetCore.Mvc;
using Models;

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
        return AsciiPinyinContext.Chachars;
    }
}
