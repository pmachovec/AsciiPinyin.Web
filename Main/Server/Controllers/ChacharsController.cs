using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Controllers;

[ApiController]
[Route($"/{ApiNames.BASE}/{ApiNames.CHARACTERS}")]
public sealed class ChacharsController(
    AsciiPinyinContext _asciiPinyinContext,
    IEntityControllerCommons _entityControllerCommons,
    ILogger<ChacharsController> _logger
) : ControllerBase, IEntityController
{
    [HttpGet]
    public ActionResult<IEnumerable<Chachar>> Get()
    {
        LogCommons.LogHttpMethodInfo(_logger, HttpMethod.Get, Actions.GET_ALL_CHACHARS);

        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(_logger, userAgent!);
        LogCommons.LogActionInDbInfo(_logger, DbActions.SELECT, Actions.GET_ALL_CHACHARS);

        try
        {
            var chachars = _asciiPinyinContext.Chachars;
            LogCommons.LogActionInDbSuccessInfo(_logger, DbActions.SELECT);
            return Ok(chachars);
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(_logger, DbActions.SELECT);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ActionResult<IErrorsContainer> Post(Chachar chachar) =>
        _entityControllerCommons.Post(
            this,
            chachar,
            _logger,
            TableNames.CHACHAR,
            Actions.CREATE_NEW_CHACHAR,
            DbActions.INSERT,
            AlterDbMethods.ADD,
            GetPostDatabaseIntegrityErrorsContainer,
            GetTheCharacterError,
            GetStrokesError,
            GetPinyinError,
            GetToneError,
            GetIpaError,
            GetRadicalCharacterError,
            GetRadicalPinyinError,
            GetRadicalToneError,
            GetRadicalAlternativeCharacterError
        );


    [HttpPost(ApiNames.DELETE)]
    public ActionResult<IErrorsContainer> PostDelete(Chachar chachar) =>
        _entityControllerCommons.Post(
            this,
            chachar,
            _logger,
            TableNames.CHACHAR,
            Actions.DELETE_CHACHAR,
            DbActions.DELETE,
            AlterDbMethods.REMOVE,
            GetPostDeleteDatabaseIntegrityErrorsContainer,
            GetTheCharacterError,
            GetPinyinError,
            GetToneError
        );

    private FieldError? GetTheCharacterError(Chachar chachar) =>
        _entityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.TheCharacter,
            JsonPropertyNames.THE_CHARACTER,
            _entityControllerCommons.GetTheCharacterErrorMessage
        );

    private FieldError? GetStrokesError(Chachar chachar) =>
        _entityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Strokes,
            JsonPropertyNames.STROKES,
            _entityControllerCommons.GetStrokesErrorMessage
        );

    private FieldError? GetPinyinError(Chachar chachar) =>
        _entityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Pinyin,
            JsonPropertyNames.PINYIN,
            _entityControllerCommons.GetPinyinErrorMessage
        );

    private FieldError? GetToneError(Chachar chachar) =>
        _entityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Tone,
            JsonPropertyNames.TONE,
            _entityControllerCommons.GetToneErrorMessage
        );

    private FieldError? GetIpaError(Chachar chachar)
    {
        string? errorMessage = null;

        if (chachar.Ipa is null)
        {
            errorMessage = Errors.MISSING;
        }
        else if (chachar.Ipa!.Length == 0)
        {
            errorMessage = Errors.EMPTY;
        }
        else if (!TextUtils.IsOnlyIpaCharacters(chachar.Ipa!))
        {
            errorMessage = Errors.NO_IPA;
        }

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(_logger, chachar.Ipa, JsonPropertyNames.IPA, errorMessage);
            return new FieldError(JsonPropertyNames.IPA, chachar.Ipa, errorMessage);
        }

        return null;
    }

    private FieldError? GetRadicalCharacterError(Chachar chachar)
    {
        string? errorMessage = null;

        if (
            chachar.RadicalCharacter is null
            && (chachar.RadicalPinyin is not null || chachar.RadicalTone is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            errorMessage = Errors.MISSING;
        }

        if (chachar.RadicalCharacter is { } radicalCharacter)
        {
            if (radicalCharacter.Length == 0)
            {
                errorMessage = Errors.EMPTY;
            }
            else if (TextUtils.GetStringRealLength(radicalCharacter) > 1)
            {
                errorMessage = Errors.ONLY_ONE_CHARACTER_ALLOWED;
            }
            else if (!TextUtils.IsOnlyChineseCharacters(radicalCharacter))
            {
                errorMessage = Errors.NO_SINGLE_CHINESE;
            }
        }

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalCharacter, JsonPropertyNames.RADICAL_CHARACTER, errorMessage);
            return new FieldError(JsonPropertyNames.RADICAL_CHARACTER, chachar.RadicalCharacter, errorMessage);
        }

        return null;
    }

    private FieldError? GetRadicalPinyinError(Chachar chachar)
    {
        string? errorMessage = null;

        if (
            chachar.RadicalPinyin is null
            && (chachar.RadicalCharacter is not null || chachar.RadicalTone is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            errorMessage = Errors.MISSING;
        }

        if (chachar.RadicalPinyin is { } radicalPinyin)
        {
            if (radicalPinyin.Length == 0)
            {
                errorMessage = Errors.EMPTY;
            }
            else if (!Regexes.AsciiLettersRegex().IsMatch(radicalPinyin))
            {
                errorMessage = Errors.NO_ASCII;
            }
        }

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalPinyin, JsonPropertyNames.RADICAL_PINYIN, errorMessage);
            return new FieldError(JsonPropertyNames.RADICAL_PINYIN, chachar.RadicalPinyin, errorMessage);
        }

        return null;
    }

    private FieldError? GetRadicalToneError(Chachar chachar)
    {
        string? errorMessage = null;

        if (
            chachar.RadicalTone is null
            && (chachar.RadicalCharacter is not null || chachar.RadicalPinyin is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            errorMessage = Errors.MISSING;
        }

        if (chachar.RadicalTone is { } radicalTone && radicalTone > 4)
        {
            // As the type is unsigned byte, API doesn't allow to pass any invalid value like strings, negative numbers etc.
            errorMessage = Errors.ZERO_TO_FOUR;
        }

        if (errorMessage is not null)
        {
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalTone, JsonPropertyNames.RADICAL_TONE, errorMessage);
            return new FieldError(JsonPropertyNames.RADICAL_TONE, chachar.RadicalTone, errorMessage);
        }

        return null;
    }

    private FieldError? GetRadicalAlternativeCharacterError(Chachar chachar)
    {
        if (chachar.RadicalAlternativeCharacter is { } radicalAlternativeCharacter)
        {
            string? errorMessage = null;

            if (radicalAlternativeCharacter.Length == 0)
            {
                errorMessage = Errors.EMPTY;
            }
            else if (TextUtils.GetStringRealLength(radicalAlternativeCharacter) > 1)
            {
                errorMessage = Errors.ONLY_ONE_CHARACTER_ALLOWED;
            }
            else if (!TextUtils.IsOnlyChineseCharacters(radicalAlternativeCharacter))
            {
                errorMessage = Errors.NO_SINGLE_CHINESE;
            }

            if (errorMessage is not null)
            {
                LogCommons.LogInvalidValueError(_logger, chachar.RadicalAlternativeCharacter, JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER, errorMessage);
                return new FieldError(JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER, chachar.RadicalAlternativeCharacter, errorMessage);
            }
        }

        return null;
    }

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
