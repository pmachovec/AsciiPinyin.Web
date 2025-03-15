using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
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
        var errorActionResult = _postFilterCommons.GetErrorActionResult<ChacharPostFilter, Chachar>(
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

    private IEnumerable<DatabaseIntegrityError> GetDatabaseIntegrityErrors(
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (chachar.RadicalCharacter is { } radicalCharacter)
        {
            var radicalChachar = knownChachars.Find(
                radicalCharacter,
                chachar.RadicalPinyin,
                chachar.RadicalTone
            );

            if (radicalChachar is null)
            {
                LogCommons.LogEntityError(_logger, Errors.RADICAL_UNKNOWN, TableNames.CHACHAR, chachar);
                return [new(Errors.RADICAL_UNKNOWN)];
            }

            if (!radicalChachar!.IsRadical)
            {
                LogCommons.LogEntityError(_logger, Errors.RADICAL_NOT_RADICAL, TableNames.CHACHAR, chachar, $"conflict chachar: {radicalChachar}");
                return [new(Errors.RADICAL_NOT_RADICAL, [new(TableNames.CHACHAR, radicalChachar)])];
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
                    LogCommons.LogEntityError(_logger, Errors.ALTERNATIVE_UNKNOWN, TableNames.CHACHAR, chachar);
                    return [new(Errors.ALTERNATIVE_UNKNOWN)];
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
            LogCommons.LogEntityError(_logger, Errors.CHACHAR_EXISTS, TableNames.CHACHAR, chachar, $"conflict chachar: {existingChachar}");
            return [new(Errors.CHACHAR_EXISTS, [new(TableNames.CHACHAR, existingChachar)])];
        }

        return [];
    }
}
