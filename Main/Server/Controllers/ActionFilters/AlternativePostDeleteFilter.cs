using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
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
        LogCommons.LogPostActionInitiatedInfo(_logger, Actions.CREATE, TableNames.ALTERNATIVE);

        var errorActionResult = _postFilterCommons.GetErrorActionResult<AlternativePostDeleteFilter, Alternative>(
            context,
            TableNames.ALTERNATIVE,
            _logger,
            AreConflictDbIntegrityErrors
        );

        if (errorActionResult is not null)
        {
            context.Result = errorActionResult;
        }
    }

    private bool AreConflictDbIntegrityErrors(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives,
        out IEnumerable<string> errors
    )
    {
        LogCommons.LogEntityInfo(_logger, TableNames.ALTERNATIVE, alternative);

        var alternativeExists = knownAlternatives
            .AsNoTracking()
            .AsEnumerable()
            .Any(knownAlternative => knownAlternative.AreAllFieldsEqual(alternative));

        if (!alternativeExists)
        {
            LogCommons.LogError(_logger, Errors.ALTERNATIVE_UNKNOWN);
            errors = [Errors.ALTERNATIVE_UNKNOWN];
            return false;
        }

        var chacharsWithThis = knownChachars.Where(knownChachar =>
            knownChachar.RadicalAlternativeCharacter == alternative.TheCharacter
            && knownChachar.RadicalCharacter == alternative.OriginalCharacter
            && knownChachar.RadicalPinyin == alternative.OriginalPinyin
            && knownChachar.RadicalTone == alternative.OriginalTone
        );

        if (chacharsWithThis.Any())
        {
            LogCommons.LogError(_logger, Errors.IS_ALTERNATIVE_FOR_CHACHARS);
            LogCommons.LogConflictsError(_logger, TableNames.CHACHAR, $"[{string.Join(",", chacharsWithThis)}]");
            errors = [Errors.IS_ALTERNATIVE_FOR_CHACHARS];
            return true;
        }

        errors = [];
        return false;
    }
}
