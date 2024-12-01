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
    public ActionResult<FieldErrorsContainer> Post(Chachar chachar)
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
            LogCommons.LogFieldErrorsContainerError(_logger, postDatabaseIntegrityErrorsContainer);
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
            ColumnNames.THE_CHARACTER,
            EntityControllerCommons.GetCharacterErrorMessage
        );

    private FieldError? GetStrokesError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Strokes,
            ColumnNames.STROKES,
            EntityControllerCommons.GetStrokesErrorMessage
        );

    private FieldError? GetPinyinError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Pinyin,
            ColumnNames.PINYIN,
            EntityControllerCommons.GetPinyinErrorMessage
        );

    private FieldError? GetToneError(Chachar chachar) =>
        EntityControllerCommons.GetInvalidValueFieldError(
            _logger,
            chachar.Tone,
            ColumnNames.TONE,
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
            LogCommons.LogInvalidValueError(_logger, chachar.Ipa, ColumnNames.IPA, errorMessage);
            return new FieldError(chachar.Ipa, errorMessage, ColumnNames.IPA);
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
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalCharacter, ColumnNames.RADICAL_CHARACTER, errorMessage);
            return new FieldError(chachar.RadicalCharacter, errorMessage, ColumnNames.RADICAL_CHARACTER);
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
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalPinyin, ColumnNames.RADICAL_PINYIN, errorMessage);
            return new FieldError(chachar.RadicalPinyin, errorMessage, ColumnNames.RADICAL_PINYIN);
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
            LogCommons.LogInvalidValueError(_logger, chachar.RadicalTone, ColumnNames.RADICAL_TONE, errorMessage);
            return new FieldError(chachar.RadicalTone, errorMessage, ColumnNames.RADICAL_TONE);
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
                LogCommons.LogInvalidValueError(_logger, chachar.RadicalAlternativeCharacter, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER, errorMessage);
                return new FieldError(chachar.RadicalAlternativeCharacter, errorMessage, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER);
            }
        }

        return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional return just looks terrible here."
    )]
    private FieldErrorsContainer? GetPostDatabaseIntegrityErrorsContainer(
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
                return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                    _logger,
                    Errors.UNKNOWN_CHACHAR,
                    (radicalCharacter, ColumnNames.RADICAL_CHARACTER),
                    (chachar.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
                    (chachar.RadicalTone, ColumnNames.RADICAL_TONE)
                );
            }

            if (!radicalChachar!.IsRadical)
            {
                return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                    _logger,
                    Errors.NO_RADICAL,
                    (radicalCharacter, ColumnNames.RADICAL_CHARACTER),
                    (chachar.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
                    (chachar.RadicalTone, ColumnNames.RADICAL_TONE)
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
                    return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                        _logger,
                        Errors.UNKNOWN_ALTERNATIVE,
                        (radicalAlternativeCharacter, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER),
                        (chachar.RadicalCharacter, ColumnNames.RADICAL_CHARACTER),
                        (chachar.RadicalPinyin, ColumnNames.RADICAL_PINYIN),
                        (chachar.RadicalTone, ColumnNames.RADICAL_TONE)
                    );
                }
            }
        }

        if (knownChachars.Contains(chachar))
        {
            return EntityControllerCommons.GetInvalidValueFieldErrorsContainer(
                _logger,
                Errors.CHACHAR_ALREADY_EXISTS,
                (chachar.TheCharacter, ColumnNames.THE_CHARACTER),
                (chachar.Pinyin, ColumnNames.PINYIN),
                (chachar.Tone, ColumnNames.TONE)
            );
        }

        return null;
    }
}
