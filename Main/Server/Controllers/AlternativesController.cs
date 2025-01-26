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
            LogCommons.LogActionInDbFailedError(_logger, DbActions.SELECT);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ActionResult<IErrorsContainer> Post(Alternative alternative) =>
        EntityControllerCommons.Post(
            _logger,
            Request,
            alternative,
            TableNames.ALTERNATIVE,
            Actions.CREATE_NEW_ALTERNATIVE,
            DbActions.INSERT,
            ContextCollectionNames.ALTERNATIVES,
            DbSetMethods.ADD,
            _asciiPinyinContext,
            BadRequest,
            StatusCode,
            Ok,
            GetPostDatabaseIntegrityErrorsContainer,
            GetTheCharacterError,
            GetStrokesError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

    [HttpPost(ApiNames.DELETE)]
    public ActionResult<IErrorsContainer> PostDelete(Alternative alternative) =>
        EntityControllerCommons.Post(
            _logger,
            Request,
            alternative,
            TableNames.ALTERNATIVE,
            Actions.DELETE_ALTERNATIVE,
            DbActions.DELETE,
            ContextCollectionNames.ALTERNATIVES,
            DbSetMethods.REMOVE,
            _asciiPinyinContext,
            BadRequest,
            StatusCode,
            Ok,
            GetPostDeleteDatabaseIntegrityErrorsContainer,
            GetTheCharacterError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

    private FieldError? GetTheCharacterError(Alternative alternative) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            alternative.TheCharacter,
            JsonPropertyNames.THE_CHARACTER,
            EntityControllerCommons.GetTheCharacterErrorMessage
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
            EntityControllerCommons.GetTheCharacterErrorMessage
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

    private DatabaseIntegrityErrorsContainer? GetPostDatabaseIntegrityErrorsContainer(
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

    private DatabaseIntegrityErrorsContainer? GetPostDeleteDatabaseIntegrityErrorsContainer(
        Alternative alternative,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (!knownAlternatives.Contains(alternative))
        {
            var errorMessage = EntityControllerCommons.GetEntityUnknownErrorMessage(
                TableNames.ALTERNATIVE,
                JsonPropertyNames.THE_CHARACTER,
                JsonPropertyNames.ORIGINAL_CHARACTER,
                JsonPropertyNames.ORIGINAL_PINYIN,
                JsonPropertyNames.ORIGINAL_TONE
            );

            return new DatabaseIntegrityErrorsContainer(
                alternative,
                errorMessage
            );
        }

        List<DatabaseIntegrityError> databaseIntegrityErrors = [];

        var chacharsWithThis = knownChachars.Where(knownChachar =>
            knownChachar.RadicalAlternativeCharacter == alternative.TheCharacter
            && knownChachar.RadicalCharacter == alternative.OriginalCharacter
            && knownChachar.RadicalPinyin == alternative.OriginalPinyin
            && knownChachar.RadicalTone == alternative.OriginalTone
        );

        if (chacharsWithThis.Any())
        {
            LogCommons.LogEntityError(
                _logger,
                Errors.IS_ALTERNATIVE_FOR_CHACHARS,
                TableNames.ALTERNATIVE,
                alternative,
                $"conflict chachars: [{string.Join(",", chacharsWithThis)}]"
            );

            return new DatabaseIntegrityErrorsContainer(
                alternative,
                Errors.IS_ALTERNATIVE_FOR_CHACHARS,
                [.. chacharsWithThis.Select(chachar => new ConflictEntity(TableNames.CHACHAR, chachar))]
            );
        }

        return null;
    }
}
