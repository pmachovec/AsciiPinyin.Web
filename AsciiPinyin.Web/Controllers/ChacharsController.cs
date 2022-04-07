using AsciiPinyin.Web.Models;
using AsciiPinyin.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Controllers
{
    [ApiController]
    [Route("/characters")]
    public class ChacharsController : ControllerBase
    {
        public ChacharsController(ChacharJsonService chacharJsonService)
        {
            this.ChacharJsonService = chacharJsonService;
        }

        private ChacharJsonService ChacharJsonService { get; }

        [HttpGet]
        public IEnumerable<Chachar> Get()
        {
            return ChacharJsonService.GetChachars();
        }
    }
}
