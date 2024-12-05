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
            LogCommons.LogActionInDbFailedError(_logger, Actions.GET_ALL_CHACHARS);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ActionResult<IErrorsContainer> Post(Chachar chachar)
    {
        LogCommons.LogHttpMethodInfo(_logger, HttpMethod.Post, Actions.CREATE_NEW_CHACHAR);

        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogUserAgentInfo(_logger, userAgent!);
        LogCommons.LogEntityInfo(_logger, nameof(Chachar), chachar);
        LogCommons.LogInitialIntegrityVerificationDebug(_logger);

        var postInitialDataErrorsContainer = EntityControllerCommons.GetPostInitialDataErrorsContainer(
            chachar,
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

        if (postInitialDataErrorsContainer is not null)
        {
            LogCommons.LogFieldErrorsContainerError(_logger, postInitialDataErrorsContainer);
            return BadRequest(postInitialDataErrorsContainer);
        }

        LogCommons.LogDatabaseIntegrityVerificationDebug(_logger);
        DbSet<Chachar>? knownChachars;
        DbSet<Alternative>? knownAlternatives;

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

        var postDatabaseIntegrityErrorsContainer = GetPostDatabaseIntegrityErrorsContainer(
            chachar,
            knownChachars,
            knownAlternatives
        );

        if (postDatabaseIntegrityErrorsContainer is not null)
        {
            LogCommons.LogDatabaseIntegrityErrorsContainerError(_logger, postDatabaseIntegrityErrorsContainer);
            return BadRequest(postDatabaseIntegrityErrorsContainer);
        }

        LogCommons.LogIntegrityVerificationSuccessDebug(_logger);
        LogCommons.LogActionInDbInfo(_logger, DbActions.INSERT, Actions.CREATE_NEW_CHACHAR);

        try
        {
            using var dbContextTransaction = _asciiPinyinContext.Database.BeginTransaction();
            _ = _asciiPinyinContext.Chachars.Add(chachar);
            _ = _asciiPinyinContext.SaveChanges();
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            LogCommons.LogActionInDbFailedError(_logger, Actions.CREATE_NEW_CHACHAR);
            LogCommons.LogError(_logger, e.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        LogCommons.LogActionInDbSuccessInfo(_logger, DbActions.INSERT);
        return Ok();
    }

    private FieldError? GetTheCharacterError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.TheCharacter,
            JsonPropertyNames.THE_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetStrokesError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Strokes,
            JsonPropertyNames.STROKES,
            EntityControllerCommons.GetStrokesErrorMessage
        );

    private FieldError? GetPinyinError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Pinyin,
            JsonPropertyNames.PINYIN,
            EntityControllerCommons.GetPinyinErrorMessage
        );

    private FieldError? GetToneError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Tone,
            JsonPropertyNames.TONE,
            EntityControllerCommons.GetToneErrorMessage
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
            else if (radicalPinyin.Length == 0)
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
                var errorMessage = EntityControllerCommons.GetEntityUnknownErrorMessage(
                    TableNames.CHACHAR,
                    JsonPropertyNames.RADICAL_CHARACTER,
                    JsonPropertyNames.RADICAL_PINYIN,
                    JsonPropertyNames.RADICAL_TONE
                );

                LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar);

                return new DatabaseIntegrityErrorsContainer(
                    TableNames.CHACHAR,
                    chachar,
                    errorMessage
                );
            }

            if (!radicalChachar!.IsRadical)
            {
                var errorMessage = EntityControllerCommons.GetNoRadicalErrorMessage(
                    JsonPropertyNames.RADICAL_CHARACTER,
                    JsonPropertyNames.RADICAL_PINYIN,
                    JsonPropertyNames.RADICAL_TONE
                );

                LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar, $"conflict chachar: {radicalChachar}");

                return new DatabaseIntegrityErrorsContainer(
                    TableNames.CHACHAR,
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
                    var errorMessage = EntityControllerCommons.GetEntityUnknownErrorMessage(
                        TableNames.ALTERNATIVE,
                        JsonPropertyNames.RADICAL_ALTERNATIVE_CHARACTER,
                        JsonPropertyNames.RADICAL_CHARACTER,
                        JsonPropertyNames.RADICAL_PINYIN,
                        JsonPropertyNames.RADICAL_TONE
                    );

                    LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar);

                    return new DatabaseIntegrityErrorsContainer(
                        TableNames.CHACHAR,
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
            var errorMessage = EntityControllerCommons.GetEntityExistsErrorMessage(
                TableNames.CHACHAR,
                JsonPropertyNames.THE_CHARACTER,
                JsonPropertyNames.PINYIN,
                JsonPropertyNames.TONE
            );

            LogCommons.LogEntityError(_logger, errorMessage, TableNames.CHACHAR, chachar, $"conflict chachar: {existingChachar}");

            return new DatabaseIntegrityErrorsContainer(
                TableNames.CHACHAR,
                chachar,
                errorMessage,
                new ConflictEntity(TableNames.CHACHAR, existingChachar)
            );
        }

        return null;
    }
}
