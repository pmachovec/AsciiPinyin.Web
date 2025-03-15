using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Commons;

internal sealed class PostFilterCommons(AsciiPinyinContext _asciiPinyinContext) : IPostFilterCommons
{
    public IActionResult? GetErrorActionResult<T1, T2>(
        ActionExecutingContext context,
        string tableName,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, IEnumerable<DatabaseIntegrityError>> getDatabaseIntegrityErrors
    ) where T1 : IPostFilter where T2 : IEntity
    {
        if (
            !GetEntityFromContext<T1, T2>(context, tableName, logger, out var chachar)
            || !GetKnownEntities(logger, out var knownChachars, out var knownAlternatives)
        )
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        else
        {
            var databaseIntegrityErrors = getDatabaseIntegrityErrors(chachar, knownChachars, knownAlternatives);

            if (databaseIntegrityErrors.Any())
            {
                // if none of errors contains conflicts, return BadRequest response
                //     if there's only one error, set its message as the response content
                //     else set array with error messages as content
                // else return Conflict response
                //     if there's only one error, set it as response content
                //     else set array with errors as content
                return databaseIntegrityErrors.All(error => !error.Conflicts.Any())
                    ? databaseIntegrityErrors.Count() == 1
                        ? new BadRequestObjectResult(databaseIntegrityErrors.First().Error)
                        : new BadRequestObjectResult(databaseIntegrityErrors.Select(error => error.Error))
                    : databaseIntegrityErrors.Count() == 1
                        ? new ConflictObjectResult(databaseIntegrityErrors.First())
                        : new ConflictObjectResult(databaseIntegrityErrors);
            }
        }

        return null;
    }

    private bool GetEntityFromContext<T1, T2>(
        ActionExecutingContext context,
        string tableName,
        ILogger<T1> logger,
        out T2 entity
    ) where T1 : IPostFilter where T2 : IEntity
    {
        var chacharObject = context.ActionArguments[tableName];

        if (chacharObject is null)
        {
            LogCommons.LogEntityNullError(logger, nameof(T2));
            entity = default!;
            return false;
        }
        else if (chacharObject is not Chachar)
        {
            LogCommons.LogEntityMismatchError(logger, nameof(T2), chacharObject.GetType().Name);
            entity = default!;
            return false;
        }

        entity = (T2)chacharObject;
        return true;
    }

    private bool GetKnownEntities<T>(
        ILogger<T> logger,
        out DbSet<Chachar> knownChachars,
        out DbSet<Alternative> knownAlternatives
    ) where T : IPostFilter
    {
        try
        {
            knownChachars = _asciiPinyinContext.Chachars;
            knownAlternatives = _asciiPinyinContext.Alternatives;
        }
        catch (Exception e)
        {
            LogCommons.LogError(logger, e.ToString());
            knownChachars = default!;
            knownAlternatives = default!;
            return false;
        }

        return true;
    }
}
