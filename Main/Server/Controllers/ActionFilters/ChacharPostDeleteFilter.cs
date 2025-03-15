using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
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
        var errorActionResult = _postFilterCommons.GetErrorActionResult<ChacharPostDeleteFilter, Chachar>(
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
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (!knownChachars.Contains(chachar))
        {
            LogCommons.LogEntityError(_logger, Errors.CHACHAR_UNKNOWN, TableNames.CHACHAR, chachar);
            return [new(Errors.CHACHAR_UNKNOWN, [])];
        }

        List<DatabaseIntegrityError> databaseIntegrityErrors = [];

        if (chachar.IsRadical)
        {
            var chacharsWithThisAsRadical = knownChachars.Where(knownChachar =>
                knownChachar.RadicalCharacter == chachar.TheCharacter
                && knownChachar.RadicalPinyin == chachar.Pinyin
                && knownChachar.RadicalTone == chachar.Tone
            );

            if (chacharsWithThisAsRadical.Any())
            {
                LogCommons.LogEntityError(
                    _logger,
                    Errors.IS_RADICAL_FOR_OTHERS,
                    TableNames.CHACHAR,
                    chachar,
                    $"conflict chachars: [{string.Join(",", chacharsWithThisAsRadical)}]"
                );

                databaseIntegrityErrors.Add(
                    new(
                        Errors.IS_RADICAL_FOR_OTHERS,
                        chacharsWithThisAsRadical.Select(chacharWithThisAsRadical => new Conflict(TableNames.CHACHAR, chacharWithThisAsRadical))
                    )
                );
            }
        }

        var alternativesOfThis = knownAlternatives.Where(knownAlternative =>
            knownAlternative.OriginalCharacter == chachar.TheCharacter
            && knownAlternative.OriginalPinyin == chachar.Pinyin
            && knownAlternative.OriginalTone == chachar.Tone
        );

        if (alternativesOfThis.Any())
        {
            LogCommons.LogEntityError(
                _logger,
                Errors.HAS_ALTERNATIVES,
                TableNames.CHACHAR,
                chachar,
                $"conflict alternatives: [{string.Join(",", alternativesOfThis)}]"
            );

            databaseIntegrityErrors.Add(
                new DatabaseIntegrityError(
                    Errors.HAS_ALTERNATIVES,
                    alternativesOfThis.Select(alternative => new Conflict(TableNames.ALTERNATIVE, alternative))
                )
            );
        }

        return databaseIntegrityErrors;
    }
}
