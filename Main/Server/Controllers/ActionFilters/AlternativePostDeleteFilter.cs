using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

internal sealed class AlternativePostDeleteFilter(
    IPostFilterCommons _postFilterCommons,
    ILogger<AlternativePostDeleteFilter> _logger
) : ActionFilterAttribute, IPostFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var errorActionResult = _postFilterCommons.GetErrorActionResult<AlternativePostDeleteFilter, Alternative>(
            context,
            TableNames.CHACHAR,
            _logger,
            GetDatabaseIntegrityErrors
        );

        if (errorActionResult is not null)
        {
            context.Result = errorActionResult;
        }
    }

    private List<DatabaseIntegrityError> GetDatabaseIntegrityErrors(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (!knownAlternatives.Contains(alternative))
        {
            LogCommons.LogEntityError(_logger, Errors.ALTERNATIVE_UNKNOWN, TableNames.ALTERNATIVE, alternative);
            return [new(Errors.ALTERNATIVE_UNKNOWN, [])];
        }

        List<DatabaseIntegrityError> databaseIntegrityErrors = [];

        var chacharsWithThis = knownChachars.Where(knownChachar =>
            knownChachar.RadicalAlternativeCharacter == alternative.TheCharacter
            && knownChachar.RadicalCharacter == alternative.OriginalCharacter
            && knownChachar.RadicalPinyin == alternative.OriginalPinyin
            && knownChachar.RadicalTone == alternative.OriginalTone
        );

        if (chacharsWithThis.Any())
        {
            LogCommons.LogEntityError(
                _logger,
                Errors.IS_ALTERNATIVE_FOR_CHACHARS,
                TableNames.ALTERNATIVE,
                alternative,
                $"conflict chachars: [{string.Join(",", chacharsWithThis)}]"
            );

            return [new(Errors.IS_ALTERNATIVE_FOR_CHACHARS, chacharsWithThis.Select(chachar => new Conflict(TableNames.CHACHAR, chachar)))];
        }

        return [];
    }
}
