using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public partial class ChacharFormBase : ComponentBase, IEntityForm
{
    protected EntitySelector<Alternative> AlternativeSelector { get; set; } = default!;

    protected EntitySelector<Chachar> RadicalSelector { get; set; } = default!;

    protected IEnumerable<Alternative> AvailableAlternatives = [];

    protected string? Ipa { get; set; }

    protected string? Pinyin { get; set; }

    protected string? RadicalAlternativeCharacter { get; set; }

    protected string? RadicalCharacter { get; set; }

    protected string? RadicalPinyin { get; set; }

    protected byte? RadicalTone { get; set; }

    protected byte? Tone { get; set; }

    public string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

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
    private ILogger<ChacharForm> Logger { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required IIndex Index { get; init; }

    protected override void OnInitialized() =>
        HtmlTitle = Localizer[Resource.CreateNewCharacter];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // No need to set these properties for these elements explicitly in the HTML part.
        if (AvailableAlternatives.Any())
        {
            await Task.WhenAll(
                JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_ALTERNATIVE_INPUT, CancellationToken.None),
                JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY, CancellationToken.None)
            );
        }
        else
        {
            await Task.WhenAll(
                JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_ALTERNATIVE_INPUT, CancellationToken.None),
                JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY, CancellationToken.None)
            );
        }
    }

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        ModalLowerLevel = null;
        Page = page;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    protected async Task OpenRadicalSelectorAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            RadicalSelector.OpenAsync(this, cancellationToken)
        );
    }

    protected async Task OpenAlternativeSelectorAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            AlternativeSelector.OpenAsync(this, cancellationToken)
        );
    }

    protected async Task SelectRadicalAsync(Chachar radicalChachar, CancellationToken cancellationToken)
    {
        if (
            RadicalCharacter != radicalChachar.TheCharacter
            && RadicalPinyin != radicalChachar.Pinyin
            && RadicalTone != radicalChachar.Tone
        )
        {
            RadicalCharacter = radicalChachar.TheCharacter;
            RadicalPinyin = radicalChachar.Pinyin;
            RadicalTone = radicalChachar.Tone;
            RadicalAlternativeCharacter = null;

            AvailableAlternatives = Index.Alternatives.Where(alternative =>
                alternative.OriginalCharacter == radicalChachar.TheCharacter
                && alternative.OriginalPinyin == radicalChachar.Pinyin
                && alternative.OriginalTone == radicalChachar.Tone
            );

            StateHasChanged();
        }

        await RadicalSelector.CloseAsync(cancellationToken);
    }

    protected async Task SelectAlternativeAsync(Alternative alternative, CancellationToken cancellationToken)
    {
        RadicalAlternativeCharacter = alternative.TheCharacter;
        StateHasChanged();
        await AlternativeSelector.CloseAsync(cancellationToken);
    }

    protected void ClearRadical()
    {
        RadicalCharacter = null;
        RadicalPinyin = null;
        RadicalTone = null;
        RadicalAlternativeCharacter = null;
        AvailableAlternatives = [];
        StateHasChanged();
    }

    protected void ClearAlternative()
    {
        RadicalAlternativeCharacter = null;
        StateHasChanged();
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        await EntityFormCommons.PreventMultipleCharactersAsync(
            this,
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            changeEventArgs,
            cancellationToken
        );
    }

    protected async Task PreventToneInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        Tone = await EntityFormCommons.GetCorrectNumberInputValueAsync(IDs.CHACHAR_FORM_TONE_INPUT, changeEventArgs.Value, Tone, cancellationToken);
        await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_TONE_INPUT, Tone.ToString()!, cancellationToken);
    }

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventStrokesInvalidAsync(this, IDs.CHACHAR_FORM_STROKES_INPUT, changeEventArgs, cancellationToken);

    protected async Task ClearWrongInputAsync(string inputId, string errorId, CancellationToken cancellationToken) =>
        await EntityFormCommons.ClearWrongInputAsync(inputId, errorId, cancellationToken);

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        LogCommons.LogHttpMethodInfo(Logger, HttpMethod.Post, Actions.CREATE_NEW_CHACHAR);
        LogCommons.LogInitialIntegrityVerificationDebug(Logger);

        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.CHACHAR_FORM_PINYIN_INPUT, IDs.CHACHAR_FORM_PINYIN_ERROR, GetPinyinErrorText),
            (IDs.CHACHAR_FORM_IPA_INPUT, IDs.CHACHAR_FORM_IPA_ERROR, GetIpaErrorText),
            (IDs.CHACHAR_FORM_TONE_INPUT, IDs.CHACHAR_FORM_TONE_ERROR, GetToneErrorText),
            (IDs.CHACHAR_FORM_STROKES_INPUT, IDs.CHACHAR_FORM_STROKES_ERROR, GetStrokesErrorText)
        );

        if (areAllInputsValid)
        {
            await CheckIntegrityAndSubmitAsync(
                TheCharacter!,
                Pinyin!,
                (byte)Tone!,
                Ipa!,
                (byte)Strokes!,
                RadicalCharacter,
                RadicalPinyin,
                RadicalTone,
                RadicalAlternativeCharacter,
                cancellationToken
            );
        }
    }

    private string? GetTheCharacterErrorText()
    {
        if (string.IsNullOrEmpty(TheCharacter))
        {
            LogCommons.LogInvalidFormValueError(Logger, TheCharacter, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyChineseCharacters(TheCharacter))
        {
            LogCommons.LogInvalidFormValueError(Logger, TheCharacter, IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, Errors.NO_SINGLE_CHINESE);
            return Localizer[Resource.MustBeChineseCharacter];
        }

        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to handle this case.

        return null;
    }

    private string? GetPinyinErrorText()
    {
        if (string.IsNullOrEmpty(Pinyin))
        {
            LogCommons.LogInvalidFormValueError(Logger, Pinyin, IDs.CHACHAR_FORM_PINYIN_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!Regexes.AsciiLettersRegex().IsMatch(Pinyin))
        {
            LogCommons.LogInvalidFormValueError(Logger, Pinyin, IDs.CHACHAR_FORM_PINYIN_INPUT, Errors.NO_ASCII);
            return Localizer[Resource.OnlyAsciiAllowed];
        }

        return null;
    }

    private string? GetIpaErrorText()
    {
        if (string.IsNullOrEmpty(Ipa))
        {
            LogCommons.LogInvalidFormValueError(Logger, Ipa, IDs.CHACHAR_FORM_IPA_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyIpaCharacters(Ipa))
        {
            LogCommons.LogInvalidFormValueError(Logger, Ipa, IDs.CHACHAR_FORM_IPA_INPUT, Errors.NO_IPA);
            return Localizer[Resource.OnlyIpaAllowed];
        }

        return null;
    }

    // Null tone is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetToneErrorText()
    {
        if (Tone is null)
        {
            LogCommons.LogInvalidFormValueError(Logger, Tone, IDs.CHACHAR_FORM_TONE_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }

        return null;
    }

    // Null strokes is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetStrokesErrorText()
    {
        if (Strokes is null)
        {
            LogCommons.LogInvalidFormValueError(Logger, Strokes, IDs.CHACHAR_FORM_STROKES_INPUT, Errors.EMPTY);
            return Localizer[Resource.CompulsoryValue];
        }

        return null;
    }

    private async Task CheckIntegrityAndSubmitAsync(
        string theCharacter,
        string pinyin,
        byte tone,
        string ipa,
        byte strokes,
        string? radicalCharacter,
        string? radicalPinyin,
        byte? radicalTone,
        string? radicalAlternativeCharacter,
        CancellationToken cancellationToken
    )
    {
        await Index.SubmitDialog.SetProcessingAsync(this, cancellationToken);

        var chachar = new Chachar()
        {
            Ipa = ipa,
            Pinyin = pinyin,
            RadicalAlternativeCharacter = radicalAlternativeCharacter,
            RadicalCharacter = radicalCharacter,
            RadicalPinyin = radicalPinyin,
            RadicalTone = radicalTone,
            Strokes = strokes,
            Tone = tone,
            TheCharacter = theCharacter
        };

        LogCommons.LogFormDataInfo(Logger, chachar);
        LogCommons.LogDatabaseIntegrityVerificationDebug(Logger);
        var databseIntegrityErrorText = GetDatabaseIntegrityErrorText(chachar);

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
            await SubmitAsync(chachar, cancellationToken);
        }
    }

    private string? GetDatabaseIntegrityErrorText(Chachar chachar)
    {
        if (Index.Chachars.Contains(chachar))
        {
            LogCommons.LogError(Logger, Errors.CHACHAR_ALREADY_EXISTS);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.CharacterAlreadyInDb],
                chachar.TheCharacter,
                chachar.RealPinyin
            );
        }

        return null;
    }

    private async Task SubmitAsync(Chachar chachar, CancellationToken cancellationToken)
    {
        var postTask = EntityClient.PostEntityAsync(ApiNames.CHARACTERS, chachar, cancellationToken);
        var postResult = await postTask;

        if (postResult == HttpStatusCode.OK)
        {
            LogCommons.LogHttpMethodSuccessInfo(Logger, HttpMethod.Post);
            await Index.SubmitDialog.SetSuccessAsync(
                this,
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localizer[Resource.CharacterCreated],
                    chachar.TheCharacter,
                    chachar.RealPinyin
                ),
                cancellationToken
            );

            _ = Index.Chachars.Add(chachar);
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
