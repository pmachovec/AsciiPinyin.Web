namespace AsciiPinyin.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

[ApiController]
[Route("/characters")]
public class ChacharsController : ControllerBase
{
    private ChacharJsonService ChacharJsonService { get; }

    public ChacharsController(ChacharJsonService chacharJsonService)
    {
        ChacharJsonService = chacharJsonService;
    }

    [HttpGet]
    public IEnumerable<Chachar> Get()
    {
        return ChacharJsonService.GetChachars();
    }
}
