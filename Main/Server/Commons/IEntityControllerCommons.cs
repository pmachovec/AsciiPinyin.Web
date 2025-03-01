using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Commons;

public interface IEntityControllerCommons
{
    // 'Get' is some kind of a reserved word, it works, but VS shows a warning => using 'TheGet' instead.
    ActionResult<IEnumerable<T2>> TheGet<T1, T2>(
        T1 entityController,
        ILogger<T1> logger,
        string action,
        string contextCollectionName
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;

    ActionResult<IErrorsContainer> Post<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDatabaseIntegrityErrorsContainer
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;

    ActionResult<IErrorsContainer> PostDelete<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDeleteDatabaseIntegrityErrorsContainer
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;

    string GetEntityUnknownErrorMessage(string entityType, params string[] fieldNames);

    string GetEntityExistsErrorMessage(string entityType, params string[] fieldNames);

    string GetNoRadicalErrorMessage(params string[] fieldNames);

    FieldError? GetInvalidValueFieldError<T1, T2>(
        ILogger<T1> logger,
        T2 value,
        string fieldName,
        Func<T2, string?> getErrorMessage
    ) where T1 : IEntityController;
}
