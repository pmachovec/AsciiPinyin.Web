using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Delegates;
using AsciiPinyin.Web.Shared.Constants;
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
        AreConflictDbItegrityErrors<T2> areConflictDbItegrityErrors
    ) where T1 : IPostFilter where T2 : IEntity
    {
        if (
            !GetEntityFromContext<T1, T2>(context, tableName, logger, out var chachar)
            || !GetKnownEntities(logger, out var knownChachars, out var knownAlternatives)
        )
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        else if (areConflictDbItegrityErrors(chachar, knownChachars, knownAlternatives, out var errors))
        {
            return new ConflictObjectResult(new Dictionary<string, IEnumerable<string>> { { JsonPropertyNames.ERRORS, errors } });
        }
        else if (errors.Any())
        {
            return new BadRequestObjectResult(new Dictionary<string, IEnumerable<string>> { { JsonPropertyNames.ERRORS, errors } });
        }

        return null;
    }

    private static bool GetEntityFromContext<T1, T2>(
        ActionExecutingContext context,
        string tableName,
        ILogger<T1> logger,
        out T2 entity
    ) where T1 : IPostFilter where T2 : IEntity
    {
        var entityObject = context.ActionArguments[tableName];

        if (entityObject is null)
        {
            LogCommons.LogEntityNullError(logger, typeof(T2).Name);
            entity = default!;
            return false;
        }
        else if (entityObject is not Chachar and not Alternative)
        {
            LogCommons.LogEntityMismatchError(logger, entityObject.GetType().Name);
            entity = default!;
            return false;
        }

        entity = (T2)entityObject;
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
