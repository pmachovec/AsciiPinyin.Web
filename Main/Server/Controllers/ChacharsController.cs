using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed class ChacharsController(
    IEntityControllerCommons _entityControllerCommons,
    ILogger<ChacharsController> _logger
) : ControllerBase, IEntityController
{
    [HttpGet]
    public ActionResult<IEnumerable<Chachar>> Get() =>
        _entityControllerCommons.TheGet<ChacharsController, Chachar>(
            this,
            _logger,
            Actions.GET_ALL_CHACHARS,
            nameof(AsciiPinyinContext.Chachars)
        );

    [HttpPost]
    public ActionResult<IErrorsContainer> Post(Chachar chachar) =>
        _entityControllerCommons.Post(
            this,
            chachar,
            _logger,
            GetPostDatabaseIntegrityErrorsContainer
        );


    [HttpPost(ApiNames.DELETE)]
    public ActionResult<IErrorsContainer> PostDelete(Chachar chachar) =>
        _entityControllerCommons.PostDelete(
            this,
            chachar,
            _logger,
            GetPostDeleteDatabaseIntegrityErrorsContainer
        );

    private DatabaseIntegrityErrorsContainer? GetPostDatabaseIntegrityErrorsContainer(
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (chachar.RadicalCharacter is { } radicalCharacter)
        {
            var radicalChachar = knownChachars.Find(
                radicalCharacter,
                chachar.RadicalPinyin,
                chachar.RadicalTone
            );

            if (radicalChachar is null)
            {
                var errorMessage = _entityControllerCommons.GetEntityUnknownErrorMessage(
                    TableNames.CHACHAR,
                    JsonPropertyNames.RADICAL_CHARACTER,
                    JsonPropertyNames.RADICAL_PINYIN,
                    JsonPropertyNames.RADICAL_TONE
                );

                LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar);

                return new DatabaseIntegrityErrorsContainer(
                    chachar,
                    errorMessage
                );
            }

            if (!radicalChachar!.IsRadical)
            {
                var errorMessage = _entityControllerCommons.GetNoRadicalErrorMessage(
                    JsonPropertyNames.RADICAL_CHARACTER,
                    JsonPropertyNames.RADICAL_PINYIN,
                    JsonPropertyNames.RADICAL_TONE
                );

                LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar, $"conflict chachar: {radicalChachar}");

                return new DatabaseIntegrityErrorsContainer(
                    chachar,
                    errorMessage,
                    new ConflictEntity(TableNames.CHACHAR, radicalChachar)
                );
            }

            if (chachar.RadicalAlternativeCharacter is { } radicalAlternativeCharacter)
            {
                var radicalAlternative = knownAlternatives.Find(
                   radicalAlternativeCharacter,
                   radicalChachar.TheCharacter,
                   radicalChachar.Pinyin,
                   radicalChachar.Tone
                );

                if (radicalAlternative is null)
                {
                    var errorMessage = _entityControllerCommons.GetEntityUnknownErrorMessage(
                        TableNames.ALTERNATIVE,
                        JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER,
                        JsonPropertyNames.RADICAL_CHARACTER,
                        JsonPropertyNames.RADICAL_PINYIN,
                        JsonPropertyNames.RADICAL_TONE
                    );

                    LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar);

                    return new DatabaseIntegrityErrorsContainer(
                        chachar,
                        errorMessage
                    );
                }
            }
        }

        var existingChachar = knownChachars.Find(
            chachar.TheCharacter,
            chachar.Pinyin,
            chachar.Tone
        );

        if (existingChachar is not null)
        {
            var errorMessage = _entityControllerCommons.GetEntityExistsErrorMessage(
                TableNames.CHACHAR,
                JsonPropertyNames.THE_CHARACTER,
                JsonPropertyNames.PINYIN,
                JsonPropertyNames.TONE
            );

            LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar, $"conflict chachar: {existingChachar}");

            return new DatabaseIntegrityErrorsContainer(
                chachar,
                errorMessage,
                new ConflictEntity(TableNames.CHACHAR, existingChachar)
            );
        }

        return null;
    }

    private DatabaseIntegrityErrorsContainer? GetPostDeleteDatabaseIntegrityErrorsContainer(
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (!knownChachars.Contains(chachar))
        {
            var errorMessage = _entityControllerCommons.GetEntityUnknownErrorMessage(
                TableNames.CHACHAR,
                JsonPropertyNames.THE_CHARACTER,
                JsonPropertyNames.PINYIN,
                JsonPropertyNames.TONE
            );

            return new DatabaseIntegrityErrorsContainer(
                chachar,
                errorMessage
            );
        }

        List<DatabaseIntegrityError> databaseIntegrityErrors = [];

        if (chachar.IsRadical)
        {
            var chacharsWithThisAsRadical = knownChachars.Where(knownChachar =>
                knownChachar.RadicalCharacter == chachar.TheCharacter
                && knownChachar.RadicalPinyin == chachar.Pinyin
                && knownChachar.RadicalTone == chachar.Tone
            );

            if (chacharsWithThisAsRadical.Any())
            {
                LogCommons.LogEntityError(
                    _logger,
                    Errors.IS_RADICAL_FOR_OTHERS,
                    TableNames.CHACHAR,
                    chachar,
                    $"conflict chachars: [{string.Join(",", chacharsWithThisAsRadical)}]"
                );

                databaseIntegrityErrors.Add(
                    new DatabaseIntegrityError(
                        chachar,
                        Errors.IS_RADICAL_FOR_OTHERS,
                        chacharsWithThisAsRadical.Select(chacharWithThisAsRadical => new ConflictEntity(TableNames.CHACHAR, chacharWithThisAsRadical))
                    )
                );
            }
        }

        var alternativesOfThis = knownAlternatives.Where(knownAlternative =>
            knownAlternative.OriginalCharacter == chachar.TheCharacter
            && knownAlternative.OriginalPinyin == chachar.Pinyin
            && knownAlternative.OriginalTone == chachar.Tone
        );

        if (alternativesOfThis.Any())
        {
            LogCommons.LogEntityError(
                _logger,
                Errors.HAS_ALTERNATIVES,
                TableNames.CHACHAR,
                chachar,
                $"conflict alternatives: [{string.Join(",", alternativesOfThis)}]"
            );

            databaseIntegrityErrors.Add(
                new DatabaseIntegrityError(
                    chachar,
                    Errors.HAS_ALTERNATIVES,
                    alternativesOfThis.Select(alternative => new ConflictEntity(TableNames.ALTERNATIVE, alternative))
                )
            );
        }

        return databaseIntegrityErrors.Count != 0 ? new DatabaseIntegrityErrorsContainer([.. databaseIntegrityErrors]) : null;
    }
}
