using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

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
        var possibleGetError = EntityControllerCommons.GetGetErrorWithLogging(
            ApiNames.ALTERNATIVES,
            Request.Headers,
            _logger
        );

        if (possibleGetError is { } getError)
        {
            return StatusCode(StatusCodes.Status400BadRequest, getError);
        }

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
        var possiblePostError = EntityControllerCommons.GetPostErrorWithLogging(
            alternative,
            Request.Headers,
            _logger,
            EntityControllerCommons.GetTheCharacterError,
            EntityControllerCommons.GetStrokesError,
            GetOriginalCharacterError,
            GetOriginalPinyinError,
            GetOriginalToneError
        );

        if (possiblePostError is { } postError)
        {
            return StatusCode(StatusCodes.Status400BadRequest, postError);
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
}
