using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

internal sealed class ChacharPostFilter(
    IPostFilterCommons _postFilterCommons,
    ILogger<ChacharPostFilter> _logger
) : ActionFilterAttribute, IPostFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LogCommons.LogPostActionInitiatedInfo(_logger, Actions.CREATE, TableNames.CHACHAR);

        var errorActionResult = _postFilterCommons.GetErrorActionResult<ChacharPostFilter, Chachar>(
            context,
            TableNames.CHACHAR,
            _logger,
            AreConflictDbIntegrityErrors
        );

        if (errorActionResult is not null)
        {
            context.Result = errorActionResult;
        }
    }

    private bool AreConflictDbIntegrityErrors(
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives,
        out IEnumerable<string> errors
    )
    {
        LogCommons.LogEntityInfo(_logger, TableNames.CHACHAR, chachar);

        if (chachar.RadicalCharacter is { } radicalCharacter)
        {
            var radicalChachar = knownChachars.Find(
                radicalCharacter,
                chachar.RadicalPinyin,
                chachar.RadicalTone
            );

            if (radicalChachar is null)
            {
                LogCommons.LogError(_logger, Errors.RADICAL_UNKNOWN);
                errors = [Errors.RADICAL_UNKNOWN];
                return false;
            }

            if (!radicalChachar!.IsRadical)
            {
                LogCommons.LogError(_logger, Errors.RADICAL_NOT_RADICAL);
                LogCommons.LogConflictError(_logger, TableNames.CHACHAR, radicalChachar);
                errors = [Errors.RADICAL_NOT_RADICAL];
                return true;
            }

            if (chachar.RadicalAlternativeCharacter is { } radicalAlternativeCharacter)
            {
                var radicalAlternative = knownAlternatives.Find(
                   radicalAlternativeCharacter,
                   radicalChachar.TheCharacter,
                   radicalChachar.Pinyin,
                   radicalChachar.Tone
                );

                if (radicalAlternative is null)
                {
                    LogCommons.LogError(_logger, Errors.ALTERNATIVE_UNKNOWN);
                    errors = [Errors.ALTERNATIVE_UNKNOWN];
                    return false;
                }
            }
        }

        var existingChachar = knownChachars.Find(
            chachar.TheCharacter,
            chachar.Pinyin,
            chachar.Tone
        );

        if (existingChachar is not null)
        {
            LogCommons.LogError(_logger, Errors.CHACHAR_EXISTS);
            LogCommons.LogConflictError(_logger, TableNames.CHACHAR, existingChachar);
            errors = [Errors.CHACHAR_EXISTS];
            return true;
        }

        errors = [];
        return false;
    }
}
