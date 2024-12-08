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
    public ActionResult<IErrorsContainer> Post(Alternative alternative)
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
            TableNames.ALTERNATIVE,
            alternative,
            GetTheCharacterError,
            GetStrokesError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

        if (postInitialDataErrorsContainer is not null)
        {
            LogCommons.LogFieldsErrorsContainerError(_logger, postInitialDataErrorsContainer);
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
            LogCommons.LogDatabaseIntegrityErrorsContainerError(_logger, postDatabaseIntegrityErrorsContainer);
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
            JsonPropertyNames.THE_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetStrokesError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.Strokes,
            JsonPropertyNames.STROKES,
            EntityControllerCommons.GetStrokesErrorMessage
        );

    private FieldError? GetOriginalCharacterError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalCharacter,
            JsonPropertyNames.ORIGINAL_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetOriginalPinyinError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalPinyin,
            JsonPropertyNames.ORIGINAL_PINYIN,
            EntityControllerCommons.GetPinyinErrorMessage
        );

    private FieldError? GetOriginalToneError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.OriginalTone,
            JsonPropertyNames.ORIGINAL_TONE,
            EntityControllerCommons.GetToneErrorMessage
        );

    private DatabaseIntegrityErrorsContainer? GetPostDatabaseIntegrityErrorContainer(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        var originalChachar = knownChachars.Find(
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (originalChachar is null)
        {
            var errorMessage = EntityControllerCommons.GetEntityUnknownErrorMessage(
                TableNames.CHACHAR,
                JsonPropertyNames.ORIGINAL_CHARACTER,
                JsonPropertyNames.ORIGINAL_PINYIN,
                JsonPropertyNames.ORIGINAL_TONE
            );

            LogCommons.LogEntityError(_logger, errorMessage, TableNames.ALTERNATIVE, alternative);

            return new DatabaseIntegrityErrorsContainer(
                alternative,
                errorMessage
            );
        }

        if (!originalChachar.IsRadical)
        {
            var errorMessage = EntityControllerCommons.GetNoRadicalErrorMessage(
                JsonPropertyNames.ORIGINAL_CHARACTER,
                JsonPropertyNames.ORIGINAL_PINYIN,
                JsonPropertyNames.ORIGINAL_TONE
            );

            LogCommons.LogEntityError(_logger, errorMessage, TableNames.ALTERNATIVE, alternative, $"conflict chachar: {originalChachar}");

            return new DatabaseIntegrityErrorsContainer(
                alternative,
                errorMessage,
                new ConflictEntity(TableNames.CHACHAR, originalChachar)
            );
        }

        var existingAlternative = knownAlternatives.Find(
            alternative.TheCharacter,
            alternative.OriginalCharacter,
            alternative.OriginalPinyin,
            alternative.OriginalTone
        );

        if (existingAlternative is not null)
        {
            var errorMessage = EntityControllerCommons.GetEntityExistsErrorMessage(
                TableNames.ALTERNATIVE,
                JsonPropertyNames.THE_CHARACTER,
                JsonPropertyNames.ORIGINAL_CHARACTER,
                JsonPropertyNames.ORIGINAL_PINYIN,
                JsonPropertyNames.ORIGINAL_TONE
            );

            LogCommons.LogEntityError(_logger, errorMessage, TableNames.ALTERNATIVE, alternative, $"conflict alternative: {existingAlternative}");

            return new DatabaseIntegrityErrorsContainer(
                alternative,
                errorMessage,
                new ConflictEntity(TableNames.ALTERNATIVE, existingAlternative)
            );
        }

        return null;
    }
}
