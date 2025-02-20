using AsciiPinyin.Web.Server.Locals;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.NLOGCONFIG}")]
public sealed class NLogConfigController(ILocals _locals) : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get() =>
        StatusCode(
            StatusCodes.Status200OK,
            System.IO.File.ReadAllText(_locals.NLogConfigYamlPath)
        );
}
