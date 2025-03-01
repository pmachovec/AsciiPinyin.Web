using AsciiPinyin.Web.Server.Commons;
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
    IEntityControllerCommons _entityControllerCommons,
    ILogger<AlternativesController> _logger
) : ControllerBase, IEntityController
{
    [HttpGet]
    public ActionResult<IEnumerable<Alternative>> Get() =>
        _entityControllerCommons.TheGet<AlternativesController, Alternative>(
            this,
            _logger,
            Actions.GET_ALL_ALTERNATIVES,
            nameof(AsciiPinyinContext.Alternatives)
        );

    [HttpPost]
    public ActionResult<IErrorsContainer> Post(Alternative alternative) =>
        _entityControllerCommons.Post(
            this,
            alternative,
            _logger,
            GetPostDatabaseIntegrityErrorsContainer
        );

    [HttpPost(ApiNames.DELETE)]
    public ActionResult<IErrorsContainer> PostDelete(Alternative alternative) =>
        _entityControllerCommons.PostDelete(
            this,
            alternative,
            _logger,
            GetPostDeleteDatabaseIntegrityErrorsContainer
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
            var errorMessage = _entityControllerCommons.GetEntityUnknownErrorMessage(
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
            var errorMessage = _entityControllerCommons.GetNoRadicalErrorMessage(
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
            var errorMessage = _entityControllerCommons.GetEntityExistsErrorMessage(
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
            var errorMessage = _entityControllerCommons.GetEntityUnknownErrorMessage(
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
