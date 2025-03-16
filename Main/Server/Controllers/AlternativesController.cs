using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed class AlternativesController(
    IEntityControllerCommons _entityControllerCommons,
    ILogger<AlternativesController> _logger
) : ControllerBase, IEntityController
{
    [
        HttpGet,
        ServiceFilter(typeof(AlternativeGetFilter))
    ]
    public ActionResult<IEnumerable<Alternative>> Get() =>
        _entityControllerCommons.TheGet<AlternativesController, Alternative>(
            this,
            _logger,
            nameof(AsciiPinyinContext.Alternatives)
        );

    [
        HttpPost,
        ServiceFilter(typeof(AlternativePostFilter))
    ]
    public ActionResult Post(Alternative alternative) =>
        _entityControllerCommons.Post(
            this,
            alternative,
            TableNames.ALTERNATIVE,
            _logger
        );

    [
        HttpPost(ApiNames.DELETE),
        ServiceFilter(typeof(AlternativePostDeleteFilter))
    ]
    public ActionResult PostDelete(Alternative alternative) =>
        _entityControllerCommons.PostDelete(
            this,
            alternative,
            TableNames.ALTERNATIVE,
            _logger
        );
}
