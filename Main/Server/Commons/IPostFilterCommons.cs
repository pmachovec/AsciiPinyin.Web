using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Delegates;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AsciiPinyin.Web.Server.Commons;

internal interface IPostFilterCommons
{
    IActionResult? GetErrorActionResult<T1, T2>(
        ActionExecutingContext context,
        string tableName,
        ILogger<T1> logger,
        AreConflictDbItegrityErrors<T2> isConflictDbItegrityError
    ) where T1 : IPostFilter where T2 : IEntity;
}
