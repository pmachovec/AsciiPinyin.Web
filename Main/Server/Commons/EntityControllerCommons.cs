using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AsciiPinyin.Web.Server.Commons;

public sealed class EntityControllerCommons(AsciiPinyinContext _asciiPinyinContext) : IEntityControllerCommons
{
    public ActionResult<IEnumerable<T2>> TheGet<T1, T2>(
        T1 entityController,
        ILogger<T1> logger,
        string action,
        string contextCollectionName
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        LogCommons.LogActionInDbInfo(logger, DbActions.SELECT, action);

        try
        {
            dynamic contextCollection = _asciiPinyinContext.GetType().GetProperty(contextCollectionName)!.GetValue(_asciiPinyinContext)!;
            LogCommons.LogActionInDbSuccessInfo(logger, DbActions.SELECT);
            return entityController.Ok(contextCollection);
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(logger, DbActions.SELECT);
            LogCommons.LogError(logger, e.ToString());
            return entityController.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public ActionResult<IErrorsContainer> Post<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDatabaseIntegrityErrorsContainer
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity =>
        PostCommon(
            entityController,
            entity,
            logger,
            $"create new {nameof(T2)}",
            "INSERT",
            _asciiPinyinContext.Add,
            getPostDatabaseIntegrityErrorsContainer
        );

    public ActionResult<IErrorsContainer> PostDelete<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDeleteDatabaseIntegrityErrorsContainer
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity =>
        PostCommon(
            entityController,
            entity,
            logger,
            $"delete {nameof(T2)}",
            "DELETE",
            _asciiPinyinContext.Remove,
            getPostDeleteDatabaseIntegrityErrorsContainer
        );

    public string GetEntityUnknownErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' does not identify an existing {entityType}";

    public string GetEntityExistsErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies an already existing {entityType}";

    public string GetNoRadicalErrorMessage(params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies a chachar, which is not radical";

    public FieldError? GetInvalidValueFieldError<T1, T2>(
        ILogger<T1> logger,
        T2 value,
        string fieldName,
        Func<T2, string?> getErrorMessage
    ) where T1 : IEntityController
    {
        var errorMessage = getErrorMessage(value);

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(logger, value, fieldName, errorMessage);
            return new FieldError(fieldName, value, errorMessage);
        }

        return null;
    }

    private ActionResult<IErrorsContainer> PostCommon<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        string actionForLog,
        string dbActionForLog,
        Func<object, EntityEntry> alterDb,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDatabaseIntegrityErrorsContainer
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        LogCommons.LogEntityInfo(logger, nameof(T2), entity);
        LogCommons.LogDatabaseIntegrityVerificationDebug(logger);
        DbSet<Chachar>? knownChachars;
        DbSet<Alternative>? knownAlternatives;

        try
        {
            knownChachars = _asciiPinyinContext.Chachars;
            knownAlternatives = _asciiPinyinContext.Alternatives;
        }
        catch (Exception e)
        {
            LogCommons.LogError(logger, e.ToString());
            return entityController.StatusCode(StatusCodes.Status500InternalServerError);
        }

        var postDatabaseIntegrityErrorsContainer = getPostDatabaseIntegrityErrorsContainer(
            entity,
            knownChachars,
            knownAlternatives
        );

        if (postDatabaseIntegrityErrorsContainer is not null)
        {
            LogCommons.LogDatabaseIntegrityErrorsContainerError(logger, postDatabaseIntegrityErrorsContainer);
            return entityController.BadRequest(postDatabaseIntegrityErrorsContainer);
        }

        LogCommons.LogIntegrityVerificationSuccessDebug(logger);
        LogCommons.LogActionInDbInfo(logger, dbActionForLog, actionForLog);

        try
        {
            // Do not use asynchronous methods for DB alterations, synchronous have better performance.
            // Even Microsoft recommends to use the synchronous approach.
            using var dbContextTransaction = _asciiPinyinContext.Database.BeginTransaction();
            _ = alterDb(entity);
            _ = _asciiPinyinContext.SaveChanges();
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(logger, dbActionForLog);
            LogCommons.LogError(logger, e.ToString());
            return entityController.StatusCode(StatusCodes.Status500InternalServerError);
        }

        LogCommons.LogActionInDbSuccessInfo(logger, dbActionForLog);
        return entityController.Ok();
    }
}
