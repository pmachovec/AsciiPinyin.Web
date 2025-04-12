using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

internal sealed class ChacharPostDeleteFilter(
    IPostFilterCommons _postFilterCommons,
    ILogger<ChacharPostDeleteFilter> _logger
) : ActionFilterAttribute, IPostFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LogCommons.LogPostActionInitiatedInfo(_logger, Actions.DELETE, TableNames.CHACHAR);

        var errorActionResult = _postFilterCommons.GetErrorActionResult<ChacharPostDeleteFilter, Chachar>(
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

        var chacharExists = knownChachars
            .AsNoTracking()
            .AsEnumerable()
            .Any(knownChachar => knownChachar.AreAllFieldsEqual(chachar));

        if (!chacharExists)
        {
            LogCommons.LogError(_logger, Errors.CHACHAR_UNKNOWN);
            errors = [Errors.CHACHAR_UNKNOWN];
            return false;
        }

        List<string> errorList = [];

        if (chachar.IsRadical)
        {
            var chacharsWithThisAsRadical = knownChachars.Where(knownChachar =>
                knownChachar.RadicalCharacter == chachar.TheCharacter
                && knownChachar.RadicalPinyin == chachar.Pinyin
                && knownChachar.RadicalTone == chachar.Tone
            );

            if (chacharsWithThisAsRadical.Any())
            {
                LogCommons.LogError(_logger, Errors.IS_RADICAL_FOR_OTHERS);
                LogCommons.LogConflictsError(_logger, TableNames.CHACHAR, $"[{string.Join(",", chacharsWithThisAsRadical)}]");
                errorList.Add(Errors.IS_RADICAL_FOR_OTHERS);
            }
        }

        var alternativesOfThis = knownAlternatives.Where(knownAlternative =>
            knownAlternative.OriginalCharacter == chachar.TheCharacter
            && knownAlternative.OriginalPinyin == chachar.Pinyin
            && knownAlternative.OriginalTone == chachar.Tone
        );

        if (alternativesOfThis.Any())
        {
            LogCommons.LogError(_logger, Errors.HAS_ALTERNATIVES);
            LogCommons.LogConflictsError(_logger, TableNames.ALTERNATIVE, $"[{string.Join(",", alternativesOfThis)}]");
            errorList.Add(Errors.HAS_ALTERNATIVES);
        }

        errors = errorList;
        return errors.Any();
    }
}
