using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
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
        var errorActionResult = _postFilterCommons.GetErrorActionResult<AlternativePostFilter, Alternative>(
            context,
            TableNames.ALTERNATIVE,
            _logger,
            GetDatabaseIntegrityErrors
        );

        if (errorActionResult is not null)
        {
            context.Result = errorActionResult;
        }
    }

    private IEnumerable<DatabaseIntegrityError> GetDatabaseIntegrityErrors(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        var originalChachar = knownChachars.Find(
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (originalChachar is null)
        {
            LogCommons.LogEntityError(_logger, Errors.ORIGINAL_UNKNOWN, TableNames.ALTERNATIVE, alternative);
            return [new(Errors.ORIGINAL_UNKNOWN)];
        }

        if (!originalChachar.IsRadical)
        {
            LogCommons.LogEntityError(_logger, Errors.ORIGINAL_NOT_RADICAL, TableNames.ALTERNATIVE, alternative, $"conflict chachar: {originalChachar}");
            return [new(Errors.ORIGINAL_NOT_RADICAL, [new(TableNames.ALTERNATIVE, originalChachar)])];
        }

        var existingAlternative = knownAlternatives.Find(
            alternative.TheCharacter,
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (existingAlternative is not null)
        {
            LogCommons.LogEntityError(_logger, Errors.ALTERNATIVE_EXISTS, TableNames.ALTERNATIVE, alternative, $"conflict alternative: {existingAlternative}");
            return [new(Errors.ALTERNATIVE_EXISTS, [new(TableNames.ALTERNATIVE, existingAlternative)])];
        }

        return [];
    }
}
