namespace AsciiPinyin.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

[ApiController]
[Route("/characters")]
public class ChacharsController : ControllerBase
{
    public ChacharsController(ChacharJsonService chacharJsonService)
    {
        ChacharJsonService = chacharJsonService;
    }

    private ChacharJsonService ChacharJsonService { get; }

    [HttpGet]
    public IEnumerable<Chachar> Get()
    {
        return ChacharJsonService.GetChachars();
    }
}
