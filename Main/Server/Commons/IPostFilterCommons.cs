using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Commons;

internal interface IPostFilterCommons
{
    IActionResult? GetErrorActionResult<T1, T2>(
        ActionExecutingContext context,
        string tableName,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, IEnumerable<DatabaseIntegrityError>> getDatabaseIntegrityErrors
    ) where T1 : IPostFilter where T2 : IEntity;
}
