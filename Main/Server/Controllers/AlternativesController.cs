using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Exceptions;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.ALTERNATIVES}")]
public sealed class AlternativesController(
    AsciiPinyinContext _asciiPinyinContext,
    ILogger<AlternativesController> _logger
) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Alternative>> Get()
    {
        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogGetAllEntitiesInfo(_logger, ApiNames.ALTERNATIVES, userAgent!);

        try
        {
            return StatusCode(StatusCodes.Status200OK, _asciiPinyinContext.Alternatives);
        }
        catch (Exception ex)
        {
            LogCommons.LogError(_logger, ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ObjectResult Post(Alternative alternative)
    {
        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogGetAllEntitiesInfo(_logger, ApiNames.ALTERNATIVES, userAgent!);
        LogCommons.LogInitialIntegrityVerificationDebug(_logger);
        var postInitialDataErrorsContainer = EntityControllerCommons.GetPostInitialDataErrorsContainer(
            alternative,
            EntityControllerCommons.GetTheCharacterError,
            EntityControllerCommons.GetStrokesError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

        if (postInitialDataErrorsContainer is not null)
        {
            LogCommons.LogError(_logger, postInitialDataErrorsContainer.ToString());
            return BadRequest(postInitialDataErrorsContainer);
        }

        LogCommons.LogDatabaseRadicalIntegrityVerificationDebug(_logger);
        FieldErrorsContainer? postDatabaseRadicalIntegrityErrorsContainer = null;

        try
        {
            postDatabaseRadicalIntegrityErrorsContainer = GetPostDatabaseRadicalIntegrityErrorContainer(alternative);
        }
        catch (DbGetException dge)
        {
            LogCommons.LogError(_logger, dge.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, null);
        }

        if (postDatabaseRadicalIntegrityErrorsContainer is not null)
        {
            LogCommons.LogError(_logger, postDatabaseRadicalIntegrityErrorsContainer.ToString());
            return BadRequest(postDatabaseRadicalIntegrityErrorsContainer);
        }

        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }

    private static FieldError? GetOriginalCharacterError(Alternative alternative)
    {
        var errorMessage = EntityControllerCommons.GetCharacterErrorMessage(alternative.OriginalCharacter);
        return errorMessage is not null ? new FieldError(alternative.OriginalCharacter, errorMessage, ColumnNames.ORIGINAL_CHARACTER) : null;
    }

    private static FieldError? GetOriginalPinyinError(Alternative alternative)
    {
        var errorMessage = EntityControllerCommons.GetPinyinErrorMessage(alternative.OriginalPinyin);
        return errorMessage is not null ? new FieldError(alternative.OriginalPinyin, errorMessage, ColumnNames.ORIGINAL_PINYIN) : null;
    }

    private static FieldError? GetOriginalToneError(Alternative alternative)
    {
        var errorMessage = EntityControllerCommons.GetToneErrorMessage(alternative.OriginalTone);
        return errorMessage is not null ? new FieldError(alternative.OriginalTone, errorMessage, ColumnNames.ORIGINAL_TONE) : null;
    }

    private FieldErrorsContainer? GetPostDatabaseRadicalIntegrityErrorContainer(Alternative alternative)
    {
        DbSet<Chachar>? knownChachars = null;

        try
        {
            knownChachars = _asciiPinyinContext.Chachars;
        }
        catch (Exception e)
        {
            throw new DbGetException(e);
        }

        var originalChachar = knownChachars!.FirstOrDefault(knownChachar =>
            knownChachar.TheCharacter == alternative.OriginalCharacter
            && knownChachar.Pinyin == alternative.OriginalPinyin
            && knownChachar.Tone == alternative.OriginalTone
        );

        return originalChachar is null
            ? new FieldErrorsContainer(
                new(alternative.OriginalCharacter, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_CHARACTER),
                new(alternative.OriginalPinyin, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_PINYIN),
                new(alternative.OriginalTone, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_TONE)
            )
            : !originalChachar.IsRadical
            ? new FieldErrorsContainer(
                new(alternative.OriginalCharacter, Errors.NO_RADICAL, ColumnNames.ORIGINAL_CHARACTER),
                new(alternative.OriginalPinyin, Errors.NO_RADICAL, ColumnNames.ORIGINAL_PINYIN),
                new(alternative.OriginalTone, Errors.NO_RADICAL, ColumnNames.ORIGINAL_TONE)
            )
            : null;
    }
}
