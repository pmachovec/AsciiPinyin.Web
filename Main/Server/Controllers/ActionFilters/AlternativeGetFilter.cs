using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

internal sealed class AlternativeGetFilter(ILogger<AlternativeGetFilter> _logger) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context) =>
        LogCommons.LogPostActionInitiatedDebug(_logger, Actions.GET, $"{TableNames.ALTERNATIVE}s");
}
