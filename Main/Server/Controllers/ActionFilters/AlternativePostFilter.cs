using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

internal sealed class AlternativePostFilter(
    IPostFilterCommons _postFilterCommons,
    ILogger<AlternativePostFilter> _logger
) : ActionFilterAttribute, IPostFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LogCommons.LogPostActionInitiatedInfo(_logger, Actions.CREATE, TableNames.ALTERNATIVE);

        var errorActionResult = _postFilterCommons.GetErrorActionResult<AlternativePostFilter, Alternative>(
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

        var originalChachar = knownChachars.Find(
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (originalChachar is null)
        {
            LogCommons.LogError(_logger, Errors.ORIGINAL_UNKNOWN);
            errors = [Errors.ORIGINAL_UNKNOWN];
            return false;
        }

        if (!originalChachar.IsRadical)
        {
            LogCommons.LogError(_logger, Errors.ORIGINAL_NOT_RADICAL);
            LogCommons.LogConflictError(_logger, TableNames.CHACHAR, originalChachar);
            errors = [Errors.ORIGINAL_NOT_RADICAL];
            return true;
        }

        var existingAlternative = knownAlternatives.Find(
            alternative.TheCharacter,
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (existingAlternative is not null)
        {
            LogCommons.LogError(_logger, Errors.ALTERNATIVE_EXISTS);
            LogCommons.LogConflictError(_logger, TableNames.ALTERNATIVE, existingAlternative);
            errors = [Errors.ALTERNATIVE_EXISTS];
            return true;
        }

        errors = [];
        return false;
    }
}
