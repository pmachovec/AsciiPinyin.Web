using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed class AlternativesController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<AlternativesController> _logger
) : ControllerBase, IEntityController
{
    [HttpGet]
    public ActionResult<IEnumerable<Alternative>> Get()
    {
        LogCommons.LogHttpMethodInfo(_logger, HttpMethod.Get, Actions.GET_ALL_ALTERNATIVES);

        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(_logger, userAgent!);
        LogCommons.LogActionInDbInfo(_logger, DbActions.SELECT, Actions.GET_ALL_ALTERNATIVES);

        try
        {
            var alternatives = _asciiPinyinContext.Alternatives;
            LogCommons.LogActionInDbSuccessInfo(_logger, DbActions.SELECT);
            return Ok(alternatives);
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(_logger, Actions.GET_ALL_ALTERNATIVES);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ActionResult<FieldErrorsContainer> Post(Alternative alternative)
    {
        LogCommons.LogHttpMethodInfo(_logger, HttpMethod.Post, Actions.CREATE_NEW_ALTERNATIVE);

        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(_logger, userAgent!);
        LogCommons.LogEntityInfo(_logger, nameof(Alternative), alternative);
        LogCommons.LogInitialIntegrityVerificationDebug(_logger);

        var postInitialDataErrorsContainer = EntityControllerCommons.GetPostInitialDataErrorsContainer(
            alternative,
            GetTheCharacterError,
            GetStrokesError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

        if (postInitialDataErrorsContainer is not null)
        {
            LogCommons.LogFieldErrorsContainerError(_logger, postInitialDataErrorsContainer);
            return BadRequest(postInitialDataErrorsContainer);
        }

        LogCommons.LogDatabaseIntegrityVerificationDebug(_logger);
        DbSet<Chachar>? knownChachars;
        DbSet<Alternative> knownAlternatives;

        try
        {
            knownChachars = _asciiPinyinContext.Chachars;
            knownAlternatives = _asciiPinyinContext.Alternatives;
        }
        catch (Exception e)
        {
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var postDatabaseIntegrityErrorsContainer = GetPostDatabaseIntegrityErrorContainer(
            alternative,
            knownChachars,
            knownAlternatives
        );

        if (postDatabaseIntegrityErrorsContainer is not null)
        {
            LogCommons.LogFieldErrorsContainerError(_logger, postDatabaseIntegrityErrorsContainer);
            return BadRequest(postDatabaseIntegrityErrorsContainer);
        }

        LogCommons.LogIntegrityVerificationSuccessDebug(_logger);
        LogCommons.LogActionInDbInfo(_logger, DbActions.INSERT, Actions.CREATE_NEW_ALTERNATIVE);

        try
        {
            using var dbContextTransaction = _asciiPinyinContext.Database.BeginTransaction();
            _ = _asciiPinyinContext.Alternatives.Add(alternative);
            _ = _asciiPinyinContext.SaveChanges();
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(_logger, Actions.CREATE_NEW_ALTERNATIVE);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        LogCommons.LogActionInDbSuccessInfo(_logger, DbActions.INSERT);
        return Ok();
    }

    private FieldError? GetTheCharacterError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.TheCharacter,
            ColumnNames.THE_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetStrokesError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.Strokes,
            ColumnNames.STROKES,
            EntityControllerCommons.GetStrokesErrorMessage
        );

    private FieldError? GetOriginalCharacterError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalCharacter,
            ColumnNames.ORIGINAL_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetOriginalPinyinError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalPinyin,
            ColumnNames.ORIGINAL_PINYIN,
            EntityControllerCommons.GetPinyinErrorMessage
        );

    private FieldError? GetOriginalToneError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalTone,
            ColumnNames.ORIGINAL_TONE,
            EntityControllerCommons.GetToneErrorMessage
        );

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional return just looks terrible here."
    )]
    private FieldErrorsContainer? GetPostDatabaseIntegrityErrorContainer(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        var originalChachar = knownChachars!.Find(
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (originalChachar is null)
        {
            return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                _logger,
                Errors.UNKNOWN_CHACHAR,
                (alternative.OriginalCharacter, ColumnNames.ORIGINAL_CHARACTER),
                (alternative.OriginalPinyin, ColumnNames.ORIGINAL_PINYIN),
                (alternative.OriginalTone, ColumnNames.ORIGINAL_TONE)
            );
        }

        if (!originalChachar.IsRadical)
        {
            return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                _logger,
                Errors.NO_RADICAL,
                (alternative.OriginalCharacter, ColumnNames.ORIGINAL_CHARACTER),
                (alternative.OriginalPinyin, ColumnNames.ORIGINAL_PINYIN),
                (alternative.OriginalTone, ColumnNames.ORIGINAL_TONE)
            );
        }

        if (knownAlternatives.Contains(alternative))
        {
            return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                _logger,
                Errors.ALTERNATIVE_ALREADY_EXISTS,
                (alternative.TheCharacter, ColumnNames.THE_CHARACTER),
                (alternative.OriginalCharacter, ColumnNames.ORIGINAL_CHARACTER),
                (alternative.OriginalPinyin, ColumnNames.ORIGINAL_PINYIN),
                (alternative.OriginalTone, ColumnNames.ORIGINAL_TONE)
            );
        }

        return null;
    }
}
