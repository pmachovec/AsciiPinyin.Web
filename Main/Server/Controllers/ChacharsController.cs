using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
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
) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Chachar>> Get()
    {
        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogGetAllEntitiesInfo(_logger, ApiNames.CHARACTERS, userAgent!);

        try
        {
            return StatusCode(StatusCodes.Status200OK, _asciiPinyinContext.Chachars);
        }
        catch (Exception ex)
        {
            LogCommons.LogError(_logger, ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public ObjectResult Post(Chachar chachar)
    {
        if (!Request.Headers.TryGetValue(RequestHeaderKeys.USER_AGENT, out var userAgent))
        {
            LogCommons.LogUserAgentMissingError(_logger);
            return BadRequest(Errors.USER_AGENT_MISSING);
        }

        LogCommons.LogPostEntityInfo(_logger, chachar, userAgent!);
        LogCommons.LogInitialIntegrityVerificationDebug(_logger);
        var postInitialDataErrorsContainer = EntityControllerCommons.GetPostInitialDataErrorsContainer(
            chachar,
            EntityControllerCommons.GetTheCharacterError,
            EntityControllerCommons.GetStrokesError,
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
            LogCommons.LogError(_logger, postInitialDataErrorsContainer.ToString());
            return BadRequest(postInitialDataErrorsContainer);
        }

        LogCommons.LogDatabaseRadicalIntegrityVerificationDebug(_logger);
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
            return StatusCode(StatusCodes.Status500InternalServerError, null);
        }

        var postDatabaseIntegrityErrorsContainer = GetPostDatabaseIntegrityErrorContainer(
            chachar,
            knownChachars,
            knownAlternatives
        );

        if (postDatabaseIntegrityErrorsContainer is not null)
        {
            LogCommons.LogError(_logger, postDatabaseIntegrityErrorsContainer.ToString());
            return BadRequest(postDatabaseIntegrityErrorsContainer);
        }

        return StatusCode(StatusCodes.Status501NotImplemented, "POST handling not implemented");
    }

    private static FieldError? GetPinyinError(Chachar chachar)
    {
        var errorMessage = EntityControllerCommons.GetPinyinErrorMessage(chachar.Pinyin);
        return errorMessage is not null ? new FieldError(chachar.Pinyin, errorMessage, ColumnNames.PINYIN) : null;
    }

    private static FieldError? GetToneError(Chachar chachar)
    {
        var errorMessage = EntityControllerCommons.GetToneErrorMessage(chachar.Tone);
        return errorMessage is not null ? new FieldError(chachar.Tone, errorMessage, ColumnNames.TONE) : null;
    }

    private static FieldError? GetIpaError(Chachar chachar)
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

        return errorMessage is not null ? new FieldError(chachar.Ipa, errorMessage, ColumnNames.IPA) : null;
    }

    private static FieldError? GetRadicalCharacterError(Chachar chachar)
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

        return errorMessage is not null ? new FieldError(chachar.RadicalCharacter, errorMessage, ColumnNames.RADICAL_CHARACTER) : null;
    }

    private static FieldError? GetRadicalPinyinError(Chachar chachar)
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

        return errorMessage is not null ? new FieldError(chachar.RadicalPinyin, errorMessage, ColumnNames.RADICAL_PINYIN) : null;
    }

    private static FieldError? GetRadicalToneError(Chachar chachar)
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

        return errorMessage is not null ? new FieldError(chachar.RadicalTone, errorMessage, ColumnNames.RADICAL_TONE) : null;
    }

    private static FieldError? GetRadicalAlternativeCharacterError(Chachar chachar)
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
    private static FieldErrorsContainer? GetPostDatabaseIntegrityErrorContainer(
        Chachar chachar,
        DbSet<Chachar> knownChachars,
        DbSet<Alternative> knownAlternatives
    )
    {
        if (chachar.RadicalCharacter is { } radicalCharacter)
        {
            var radicalChachar = knownChachars!.Find(
                radicalCharacter,
                chachar.RadicalPinyin,
                chachar.RadicalTone
            );

            if (radicalChachar is null)
            {
                return new FieldErrorsContainer(
                    new(radicalCharacter, Errors.UNKNOWN_CHACHAR, ColumnNames.RADICAL_CHARACTER),
                    new(chachar.RadicalPinyin, Errors.UNKNOWN_CHACHAR, ColumnNames.RADICAL_PINYIN),
                    new(chachar.RadicalTone, Errors.UNKNOWN_CHACHAR, ColumnNames.RADICAL_TONE)
                );
            }

            if (!radicalChachar!.IsRadical)
            {
                return new FieldErrorsContainer(
                    new(radicalCharacter, Errors.NO_RADICAL, ColumnNames.RADICAL_CHARACTER),
                    new(chachar.RadicalPinyin, Errors.NO_RADICAL, ColumnNames.RADICAL_PINYIN),
                    new(chachar.RadicalTone, Errors.NO_RADICAL, ColumnNames.RADICAL_TONE)
                );
            }

            if (chachar.RadicalAlternativeCharacter is { } radicalAlternativeCharacter)
            {
                var radicalAlternative = knownAlternatives!.Find(
                   radicalAlternativeCharacter,
                   radicalChachar.TheCharacter,
                   radicalChachar.Pinyin,
                   radicalChachar.Tone
                );

                if (radicalAlternative is null)
                {
                    return new FieldErrorsContainer(
                        new(radicalAlternativeCharacter, Errors.UNKNOWN_ALTERNATIVE, ColumnNames.RADICAL_ALTERNATIVE_CHARACTER),
                        new(chachar.RadicalCharacter, Errors.UNKNOWN_ALTERNATIVE, ColumnNames.RADICAL_CHARACTER),
                        new(chachar.RadicalPinyin, Errors.UNKNOWN_ALTERNATIVE, ColumnNames.RADICAL_PINYIN),
                        new(chachar.RadicalTone, Errors.UNKNOWN_ALTERNATIVE, ColumnNames.RADICAL_TONE)
                    );
                }
            }
        }

        if (knownChachars.Contains(chachar))
        {
            return new FieldErrorsContainer(
                new(chachar.TheCharacter, Errors.CHACHAR_ALREADY_EXISTS, ColumnNames.THE_CHARACTER),
                new(chachar.Pinyin, Errors.CHACHAR_ALREADY_EXISTS, ColumnNames.PINYIN),
                new(chachar.Tone, Errors.CHACHAR_ALREADY_EXISTS, ColumnNames.TONE)
            );
        }

        return null;
    }
}
