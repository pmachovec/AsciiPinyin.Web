using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        LogCommons.LogHttpMethodInfo(logger, HttpMethod.Get, action);

        if (!entityController.Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(logger);
            return entityController.BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(logger, userAgent!);
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

    public ActionResult<IErrorsContainer> ThePost<T1, T2>(
        T1 entityController,
        T2 entity,
        ILogger<T1> logger,
        string tableName,
        string action,
        string dbAction,
        string alterDbMethodName,
        Func<T2, DbSet<Chachar>, DbSet<Alternative>, DatabaseIntegrityErrorsContainer?> getPostDatabaseIntegrityErrorsContainer,
        params Func<T2, FieldError?>[] getFieldErrorMethods
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity
    {
        LogCommons.LogHttpMethodInfo(logger, HttpMethod.Post, action);

        if (!entityController.Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(logger);
            return entityController.BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(logger, userAgent!);
        LogCommons.LogEntityInfo(logger, nameof(T2), entity);
        LogCommons.LogInitialIntegrityVerificationDebug(logger);

        var postInitialDataErrorsContainer = GetPostInitialDataErrorsContainer(
            tableName,
            entity,
            getFieldErrorMethods
        );

        if (postInitialDataErrorsContainer is not null)
        {
            LogCommons.LogFieldsErrorsContainerError(logger, postInitialDataErrorsContainer);
            return entityController.BadRequest(postInitialDataErrorsContainer);
        }

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
        LogCommons.LogActionInDbInfo(logger, dbAction, action);

        try
        {
            using var dbContextTransaction = _asciiPinyinContext.Database.BeginTransaction();
            dynamic contextAlterCollectionMethod = _asciiPinyinContext.GetType().GetMethod(alterDbMethodName, [typeof(T2)])!;
            _ = contextAlterCollectionMethod!.Invoke(_asciiPinyinContext, (T2[])[entity]);
            _ = _asciiPinyinContext.SaveChanges();
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(logger, dbAction);
            LogCommons.LogError(logger, e.ToString());
            return entityController.StatusCode(StatusCodes.Status500InternalServerError);
        }

        LogCommons.LogActionInDbSuccessInfo(logger, dbAction);
        return entityController.Ok();
    }

    public string? GetTheCharacterErrorMessage(string? theCharacter)
    {
        string? errorMessage = null;

        if (theCharacter is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (theCharacter!.Length == 0)
        {
            errorMessage = Errors.EMPTY;
        }
        else if (TextUtils.GetStringRealLength(theCharacter!) > 1)
        {
            errorMessage = Errors.ONLY_ONE_CHARACTER_ALLOWED;
        }
        else if (!TextUtils.IsOnlyChineseCharacters(theCharacter!))
        {
            errorMessage = Errors.NO_SINGLE_CHINESE;
        }

        return errorMessage;
    }

    public string? GetStrokesErrorMessage(byte? strokes)
    {
        string? errorMessage = null;

        if (strokes is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (strokes is < ByteConstants.MIN_STROKES or > ByteConstants.MAX_STROKES)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ONE_TO_NINETY_NINE;
        }

        return errorMessage;
    }

    public string? GetPinyinErrorMessage(string? pinyin)
    {
        string? errorMessage = null;

        if (pinyin is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (pinyin!.Length == 0)
        {
            errorMessage = Errors.EMPTY;
        }
        else if (!Regexes.AsciiLettersRegex().IsMatch(pinyin!))
        {
            errorMessage = Errors.NO_ASCII;
        }

        return errorMessage;
    }

    public string? GetToneErrorMessage(byte? tone)
    {
        string? errorMessage = null;

        if (tone is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (tone > ByteConstants.MAX_TONE)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ZERO_TO_FOUR;
        }

        return errorMessage;
    }

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

    private static EntityFieldsErrorsContainer? GetPostInitialDataErrorsContainer<T>(
        string entityType,
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldsErrors = GetFieldsErrors(
            entity,
            getFieldErrorMethods
        );

        return fieldsErrors.Count > 0 ? new EntityFieldsErrorsContainer(entityType, [.. fieldsErrors]) : null;
    }

    private static List<FieldError> GetFieldsErrors<T>(
        T entity,
        params Func<T, FieldError?>[] getFieldErrorMethods
    ) where T : IEntity
    {
        var fieldsErrors = new List<FieldError>();

        foreach (var getFieldError in getFieldErrorMethods)
        {
            if (getFieldError(entity) is { } error)
            {
                fieldsErrors.Add(error);
            }
        }

        return fieldsErrors;
    }
}
