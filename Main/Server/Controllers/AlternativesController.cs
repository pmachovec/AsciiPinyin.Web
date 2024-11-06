using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Data;
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
            return StatusCode(StatusCodes.Status500InternalServerError, null);
        }

        var postDatabaseIntegrityErrorsContainer = GetPostDatabaseIntegrityErrorContainer(
            alternative,
            knownChachars,
            knownAlternatives
        );

        if (postDatabaseIntegrityErrorsContainer is not null)
        {
            LogCommons.LogError(_logger, postDatabaseIntegrityErrorsContainer.ToString());
            return BadRequest(postDatabaseIntegrityErrorsContainer);
        }

        using (var dbContextTransaction = _asciiPinyinContext.Database.BeginTransaction())
        {
            _ = _asciiPinyinContext.Alternatives.Add(alternative);
            _ = _asciiPinyinContext.SaveChanges();
            dbContextTransaction.Commit();
        }

        return Ok(null);
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional return just looks terrible here."
    )]
    private static FieldErrorsContainer? GetPostDatabaseIntegrityErrorContainer(
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
            return new FieldErrorsContainer(
                new(alternative.OriginalCharacter, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_CHARACTER),
                new(alternative.OriginalPinyin, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_PINYIN),
                new(alternative.OriginalTone, Errors.UNKNOWN_CHACHAR, ColumnNames.ORIGINAL_TONE)
            );
        }

        if (!originalChachar.IsRadical)
        {
            return new FieldErrorsContainer(
                new(alternative.OriginalCharacter, Errors.NO_RADICAL, ColumnNames.ORIGINAL_CHARACTER),
                new(alternative.OriginalPinyin, Errors.NO_RADICAL, ColumnNames.ORIGINAL_PINYIN),
                new(alternative.OriginalTone, Errors.NO_RADICAL, ColumnNames.ORIGINAL_TONE)
            );
        }

        if (knownAlternatives.Contains(alternative))
        {
            return new FieldErrorsContainer(
                new(alternative.TheCharacter, Errors.ALTERNATIVE_ALREADY_EXISTS, ColumnNames.THE_CHARACTER),
                new(alternative.OriginalCharacter, Errors.ALTERNATIVE_ALREADY_EXISTS, ColumnNames.ORIGINAL_CHARACTER),
                new(alternative.OriginalPinyin, Errors.ALTERNATIVE_ALREADY_EXISTS, ColumnNames.ORIGINAL_PINYIN),
                new(alternative.OriginalTone, Errors.ALTERNATIVE_ALREADY_EXISTS, ColumnNames.ORIGINAL_TONE)
            );
        }

        return null;
    }
}
