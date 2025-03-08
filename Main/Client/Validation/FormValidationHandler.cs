using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Validation;

internal sealed class FormValidationHandler<T> where T : IEntityForm
{
    private readonly ILogger<T> _logger;
    private readonly IStringLocalizer<Resource> _localizer;
    private readonly ValidationMessageStore _messageStore;

    public FormValidationHandler(
        ILogger<T> logger,
        IStringLocalizer<Resource> localizer,
        EditContext editContext
    )
    {
        _logger = logger;
        _localizer = localizer;
        _messageStore = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, fieldChangedEventArgs) => ClearError(editContext, fieldChangedEventArgs.FieldIdentifier);
        editContext.OnValidationRequested += (_, _) => ValidateForm(editContext);
    }

    private void ClearError(EditContext editContext, FieldIdentifier field)
    {
        _messageStore.Clear(field);
        editContext.NotifyValidationStateChanged();
    }

    private void ValidateForm(EditContext editContext)
    {
        _messageStore.Clear();

        if (editContext.Model is Chachar chachar)
        {
            // Do not try to run these methods asynchronously and then wait for all of them.
            // The "Task.WaitAll" doesn't work in client-side Blazor.
            ValidateTheCharacter(chachar);
            ValidatePinyin(chachar);
            ValidateIpa(chachar);
            ValidateTone(chachar);
            ValidateStrokes(chachar);
        }
        else if (editContext.Model is Alternative alternative)
        {
            ValidateTheCharacter(alternative);
            ValidateStrokes(alternative);
            ValidateOriginal(alternative);
        }
        else
        {
            throw new InvalidOperationException($"Invalid model type '{editContext.Model.GetType()}'");
        }

        editContext.NotifyValidationStateChanged();
    }

    private void ValidateTheCharacter<T1>(T1 entity) where T1 : IEntity
    {
        string? theCharacterErrorText = null;

        if (string.IsNullOrEmpty(entity.TheCharacter))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.THE_CHARACTER, Errors.MISSING);
            theCharacterErrorText = _localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyChineseCharacters(entity.TheCharacter))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.THE_CHARACTER, Errors.NO_SINGLE_CHINESE);
            theCharacterErrorText = _localizer[Resource.MustBeChineseCharacter];
        }

        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to handle this case.

        if (theCharacterErrorText is not null)
        {
            var theCharacterField = new FieldIdentifier(entity, nameof(entity.TheCharacter));
            _messageStore.Add(theCharacterField, theCharacterErrorText);
        }
    }

    private void ValidateStrokes<T1>(T1 entity) where T1 : IEntity
    {
        string? strokesErrorText = null;

        // Null strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
        if (entity.Strokes is null)
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.STROKES, Errors.MISSING);
            strokesErrorText = _localizer[Resource.CompulsoryValue];
        }

        if (strokesErrorText is not null)
        {
            var strokesField = new FieldIdentifier(entity, nameof(entity.Strokes));
            _messageStore.Add(strokesField, strokesErrorText);
        }
    }

    private void ValidatePinyin(Chachar chachar)
    {
        string? pinyinErrorText = null;

        if (string.IsNullOrEmpty(chachar.Pinyin))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.PINYIN, Errors.MISSING);
            pinyinErrorText = _localizer[Resource.CompulsoryValue];
        }
        else if (!Regexes.AsciiLettersRegex().IsMatch(chachar.Pinyin))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.PINYIN, Errors.NO_ASCII);
            pinyinErrorText = _localizer[Resource.OnlyAsciiAllowed];
        }

        if (pinyinErrorText is not null)
        {
            var pinyinField = new FieldIdentifier(chachar, nameof(chachar.Pinyin));
            _messageStore.Add(pinyinField, pinyinErrorText);
        }
    }

    private void ValidateIpa(Chachar chachar)
    {
        string? ipaErrorText = null;

        if (string.IsNullOrEmpty(chachar.Ipa))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.IPA, Errors.MISSING);
            ipaErrorText = _localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyIpaCharacters(chachar.Ipa))
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.IPA, Errors.NO_IPA);
            ipaErrorText = _localizer[Resource.OnlyIpaAllowed];
        }

        if (ipaErrorText is not null)
        {
            var ipaField = new FieldIdentifier(chachar, nameof(chachar.Ipa));
            _messageStore.Add(ipaField, ipaErrorText);
        }
    }

    private void ValidateTone(Chachar chachar)
    {
        string? toneErrorText = null;

        // Null tone is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
        if (chachar.Tone is null)
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.TONE, Errors.MISSING);
            toneErrorText = _localizer[Resource.CompulsoryValue];
        }

        if (toneErrorText is not null)
        {
            var toneField = new FieldIdentifier(chachar, nameof(chachar.Tone));
            _messageStore.Add(toneField, toneErrorText);
        }
    }

    private void ValidateOriginal(Alternative alternative)
    {
        string? originalErrorText = null;

        if (alternative.OriginalCharacter is null)
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.ORIGINAL_CHARACTER, Errors.MISSING);
            originalErrorText = _localizer[Resource.CompulsoryValue];
        }

        if (alternative.OriginalPinyin is null)
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.ORIGINAL_PINYIN, Errors.MISSING);
            originalErrorText = _localizer[Resource.CompulsoryValue];
        }

        if (alternative.OriginalCharacter is null)
        {
            LogCommons.LogPairError(_logger, JsonPropertyNames.ORIGINAL_TONE, Errors.MISSING);
            originalErrorText = _localizer[Resource.CompulsoryValue];
        }

        if (originalErrorText is not null)
        {
            var originalFiled = new FieldIdentifier(alternative, nameof(alternative.OriginalCharacter));
            _messageStore.Add(originalFiled, originalErrorText);
        }
    }
}
