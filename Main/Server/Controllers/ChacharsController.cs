using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed class ChacharsController(
    IEntityControllerCommons _entityControllerCommons,
    ILogger<ChacharsController> _logger
) : ControllerBase, IEntityController
{
    [
        HttpGet,
        ServiceFilter(typeof(ChacharGetFilter))
    ]
    public ActionResult<IEnumerable<Chachar>> Get() =>
        _entityControllerCommons.TheGet<ChacharsController, Chachar>(
            this,
            _logger,
            nameof(AsciiPinyinContext.Chachars)
        );

    [
        HttpPost,
        ServiceFilter(typeof(ChacharPostFilter))
    ]
    public ActionResult Post(Chachar chachar) =>
        _entityControllerCommons.Post(
            this,
            chachar,
            TableNames.CHACHAR,
            _logger
        );


    [
        HttpPost(ApiNames.DELETE),
        ServiceFilter(typeof(ChacharPostDeleteFilter))
    ]
    public ActionResult PostDelete(Chachar chachar) =>
        _entityControllerCommons.PostDelete(
            this,
            chachar,
            TableNames.CHACHAR,
            _logger
        );
}
