using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AsciiPinyin.Web.Server.Commons;

public sealed class EntityControllerCommons(AsciiPinyinContext _asciiPinyinContext) : IEntityControllerCommons
{
    public ActionResult<IEnumerable<T2>> TheGet<T1, T2>(
        T1 entityController,
        ILogger<T1> logger,
        string contextCollectionName
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        LogCommons.LogActionInDbDebug(logger, DbActions.SELECT);

        try
        {
            dynamic contextCollection = _asciiPinyinContext.GetType().GetProperty(contextCollectionName)!.GetValue(_asciiPinyinContext)!;
            LogCommons.LogActionInDbSuccessDebug(logger, DbActions.SELECT);
            return entityController.Ok(contextCollection);
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(logger, DbActions.SELECT);
            LogCommons.LogError(logger, e.ToString());
            return entityController.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public ActionResult Post<T1, T2>(
        T1 entityController,
        T2 entity,
        string tableName,
        ILogger<T1> logger
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        var result = PostCommon(
            entityController,
            entity,
            logger,
            DbActions.INSERT,
            _asciiPinyinContext.Add
        );

        if (result is OkResult)
        {
            LogCommons.LogEntityCreatedInfo(logger, tableName);
        }

        return result;
    }

    public ActionResult PostDelete<T1, T2>(
        T1 entityController,
        T2 entity,
        string tableName,
        ILogger<T1> logger
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        var result = PostCommon(
            entityController,
            entity,
            logger,
            DbActions.DELETE,
            _asciiPinyinContext.Remove
        );

        if (result is OkResult)
        {
            LogCommons.LogEntityDeletedInfo(logger, tableName);
        }

        return result;
    }

    private ActionResult PostCommon<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        string dbActionForLog,
        Func<object, EntityEntry> alterDb
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        LogCommons.LogActionInDbInfo(logger, dbActionForLog);

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
