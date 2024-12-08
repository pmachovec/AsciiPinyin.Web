using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeFormBase : ComponentBase, IEntityForm
{
    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected string? OriginalCharacter { get; set; }

    protected string? OriginalPinyin { get; set; }

    protected byte? OriginalTone { get; set; }

    public string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    public string HtmlTitle { get; private set; } = string.Empty;

    public byte? Strokes { get; set; }

    public string? TheCharacter { get; set; }

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private ILogger<AlternativeForm> Logger { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IIndex Index { get; set; } = default!;

    protected override void OnInitialized() =>
        HtmlTitle = Localizer[Resource.CreateNewAlternative];

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        ModalLowerLevel = null;
        Page = page;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            ClearWrongInputAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, cancellationToken),
            JSInteropDOM.SetZIndexAsync(IDs.ALTERNATIVE_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            OriginalSelector.OpenAsync(this, cancellationToken)
        );

    protected async Task SelectOriginalAsync(Chachar originalChachar, CancellationToken cancellationToken)
    {
        OriginalCharacter = originalChachar.TheCharacter;
        OriginalPinyin = originalChachar.Pinyin;
        OriginalTone = originalChachar.Tone;
        StateHasChanged();
        await OriginalSelector.CloseAsync(cancellationToken);
    }

    protected async Task ClearOriginalAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.RemoveClassAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, CssClasses.BORDER_DANGER, cancellationToken),
            JSInteropDOM.RemoveTextAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, cancellationToken)
        );

        OriginalCharacter = null;
        OriginalPinyin = null;
        OriginalTone = null;
        StateHasChanged();
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventMultipleCharactersAsync(
            this,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            changeEventArgs,
            cancellationToken
        );

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventStrokesInvalidAsync(
            this,
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            changeEventArgs,
            cancellationToken
        );

    protected async Task ClearWrongInputAsync(string inputId, string errorId, CancellationToken cancellationToken) =>
        await EntityFormCommons.ClearWrongInputAsync(inputId, errorId, cancellationToken);

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        LogCommons.LogHttpMethodInfo(Logger, HttpMethod.Post, Actions.CREATE_NEW_ALTERNATIVE);
        LogCommons.LogInitialIntegrityVerificationDebug(Logger);

        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.ALTERNATIVE_FORM_STROKES_INPUT, IDs.ALTERNATIVE_FORM_STROKES_ERROR, GetStrokesErrorText),
            (IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, GetOriginalErrorText)
        );

        if (areAllInputsValid)
        {
            await CheckIntegrityAndSubmitAsync(
                TheCharacter!,
                OriginalCharacter!,
                OriginalPinyin!,
                (byte)OriginalTone!,
                (byte)Strokes!,
                cancellationToken
            );
        }
    }

    private string? GetTheCharacterErrorText()
    {
        if (string.IsNullOrEmpty(TheCharacter))
        {
            LogCommons.LogInvalidFormValueError(Logger, TheCharacter, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyChineseCharacters(TheCharacter))
        {
            LogCommons.LogInvalidFormValueError(Logger, TheCharacter, IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, Errors.NO_SINGLE_CHINESE);
            return Localizer[Resource.MustBeChineseCharacter];
        }

        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to handle this case.

        return null;
    }

    // Null strokes is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetStrokesErrorText()
    {
        if (Strokes is null)
        {
            LogCommons.LogInvalidFormValueError(Logger, Strokes, IDs.ALTERNATIVE_FORM_STROKES_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }

        return null;
    }

    private string? GetOriginalErrorText()
    {
        if (OriginalCharacter is null)
        {
            LogCommons.LogInvalidFormValueError(Logger, OriginalCharacter, IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }

        return null;
    }

    private async Task CheckIntegrityAndSubmitAsync(
        string theCharacter,
        string originalCharacter,
        string originalPinyin,
        byte originalTone,
        byte strokes,
        CancellationToken cancellationToken
    )
    {
        await Index.SubmitDialog.SetProcessingAsync(this, cancellationToken);

        var alternative = new Alternative()
        {
            OriginalCharacter = originalCharacter,
            OriginalPinyin = originalPinyin,
            OriginalTone = originalTone,
            Strokes = strokes,
            TheCharacter = theCharacter
        };

        LogCommons.LogFormDataInfo(Logger, alternative);
        LogCommons.LogDatabaseIntegrityVerificationDebug(Logger);
        var databseIntegrityErrorText = await GetDatabaseIntegrityErrorTextAsync(alternative, cancellationToken);

        if (databseIntegrityErrorText is not null)
        {
            await Index.SubmitDialog.SetErrorAsync(
                this,
                databseIntegrityErrorText,
                cancellationToken
            );
        }
        else
        {
            await SubmitAsync(alternative, cancellationToken);
        }
    }

    private async Task<string?> GetDatabaseIntegrityErrorTextAsync(Alternative alternative, CancellationToken cancellationToken)
    {
        var alternativesContainsTask = Task.Run(() => Index.Alternatives.Contains(alternative), cancellationToken);

        if (await alternativesContainsTask)
        {
            LogCommons.LogError(Logger, Errors.ALTERNATIVE_ALREADY_EXISTS);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.AlternativeAlreadyInDb],
                alternative.TheCharacter,
                alternative.OriginalCharacter,
                alternative.OriginalRealPinyin
            );
        }

        return null;
    }

    private async Task SubmitAsync(Alternative alternative, CancellationToken cancellationToken)
    {
        var postTask = EntityClient.PostEntityAsync(ApiNames.ALTERNATIVES, alternative, cancellationToken);
        var postResult = await postTask;

        if (postResult == HttpStatusCode.OK)
        {
            LogCommons.LogHttpMethodSuccessInfo(Logger, HttpMethod.Post);
            await Index.SubmitDialog.SetSuccessAsync(
                this,
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localizer[Resource.AlternativeCreated],
                    alternative.TheCharacter,
                    alternative.OriginalCharacter,
                    alternative.OriginalRealPinyin
                ),
                cancellationToken
            );

            _ = Index.Alternatives.Add(alternative);
            Index.StateHasChangedPublic();
        }
        else
        {
            LogCommons.LogHttpMethodFailedError(Logger, HttpMethod.Post);
            await Index.SubmitDialog.SetErrorAsync(
                this,
                Localizer[Resource.ProcessingError],
                cancellationToken
            );
        }
    }
}
