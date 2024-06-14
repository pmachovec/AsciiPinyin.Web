using AsciiPinyin.Web.Client.EntityClient;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public partial class ChacharFormBase : EntityFormBase
{
    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex AsciiLettersRegex();

    protected EntitySelector<Alternative> AlternativeSelector { get; set; } = default!;

    protected EntitySelector<Chachar> RadicalSelector { get; set; } = default!;

    protected IEnumerable<Alternative> AvailableAlternatives = [];

    protected string? AlternativeError { get; set; }

    protected string? Ipa { get; set; }

    protected string? Pinyin { get; set; }

    protected string? RadicalAlternativeCharacter { get; set; }

    protected string? RadicalCharacter { get; set; }

    protected string? RadicalPinyin { get; set; }

    protected byte? RadicalTone { get; set; }

    protected byte? Tone { get; set; }

    public override byte? Strokes { get; set; }

    public override string? TheCharacter { get; set; }

    public override string BackdropId { get; } = IDs.CHACHAR_FORM_BACKDROP;

    public override string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IModalWithBackdropCommons ModalWithBackdropCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required Index Index { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            AlternativeSelector.EventOnClose += async (_, _) =>
            {
                await JSInteropDOM.SetTitleAsync(Localizer[Resource.CreateNewCharacter], CancellationToken.None);
                await JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, 1, CancellationToken.None);
            };

            RadicalSelector.EventOnClose += async (_, _) =>
            {
                await JSInteropDOM.SetTitleAsync(Localizer[Resource.CreateNewCharacter], CancellationToken.None);
                await JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, 1, CancellationToken.None);
            };
        }

        // No need to set these properties for these element explicitly in the HTML part.
        if (AvailableAlternatives.Any())
        {
            await JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_ALTERNATIVE, CancellationToken.None);
            await JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CancellationToken.None);
            await JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50, CancellationToken.None);
            await JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK, CancellationToken.None);
            await JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY, CancellationToken.None);
            await JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY, CancellationToken.None);
        }
        else
        {
            await JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_ALTERNATIVE, CancellationToken.None);
            await JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CancellationToken.None);
            await JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK, CancellationToken.None);
            await JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50, CancellationToken.None);
            await JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY, CancellationToken.None);
            await JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY, CancellationToken.None);
        }
    }

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            Localizer[Resource.CreateNewCharacter],
            cancellationToken);
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);
    }

    protected async Task OpenRadicalSelectorAsync(CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, 0, cancellationToken);
        await RadicalSelector.OpenAsync(cancellationToken);
    }

    protected async Task OpenAlternativeSelectorAsync(CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, 0, cancellationToken);
        await AlternativeSelector.OpenAsync(cancellationToken);
    }

    protected async Task SelectRadicalAsync(Chachar radicalChachar, CancellationToken cancellationToken)
    {
        if (
            RadicalCharacter != radicalChachar.TheCharacter
            && RadicalPinyin != radicalChachar.Pinyin
            && RadicalTone != radicalChachar.Tone)
        {
            RadicalCharacter = radicalChachar.TheCharacter;
            RadicalPinyin = radicalChachar.Pinyin;
            RadicalTone = radicalChachar.Tone;
            RadicalAlternativeCharacter = null;

            AvailableAlternatives = Index.Alternatives.Where(alternative =>
                alternative.OriginalCharacter == radicalChachar.TheCharacter
                && alternative.OriginalPinyin == radicalChachar.Pinyin
                && alternative.OriginalTone == radicalChachar.Tone);

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
            cancellationToken);
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
        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.CHACHAR_FORM_PINYIN_INPUT, IDs.CHACHAR_FORM_PINYIN_ERROR, GetPinyinErrorText),
            (IDs.CHACHAR_FORM_IPA_INPUT, IDs.CHACHAR_FORM_IPA_ERROR, GetIpaErrorText),
            (IDs.CHACHAR_FORM_TONE_INPUT, IDs.CHACHAR_FORM_TONE_ERROR, GetToneErrorText),
            (IDs.CHACHAR_FORM_STROKES_INPUT, IDs.CHACHAR_FORM_STROKES_ERROR, GetStrokesErrorText));

        if (areAllInputsValid)
        {
            var chachar = new Chachar()
            {
                Ipa = Ipa!,
                Pinyin = Pinyin!,
                RadicalAlternativeCharacter = RadicalAlternativeCharacter!,
                RadicalCharacter = RadicalCharacter!,
                RadicalPinyin = RadicalPinyin!,
                RadicalTone = RadicalTone!,
                Strokes = (byte)Strokes!,
                Tone = (byte)Tone!,
                TheCharacter = TheCharacter!
            };

            var statusCode = await EntityClient.CreateEntityAsync(ApiNames.CHARACTERS, chachar, cancellationToken);

            if (statusCode == HttpStatusCode.OK)
            {
                // Alert OK
                // Close form
            }
            else
            {
                // Alert ERROR
                // Keep form opened
            }
        }
    }

    private string? GetTheCharacterErrorText()
    {
        if (string.IsNullOrEmpty(TheCharacter))
        {
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyChineseCharacters(TheCharacter))
        {
            return Localizer[Resource.MustBeChineseCharacter];
        }

        // Multi-character inputs are unreachable thanks to PreventMultipleCharacters, no need to handle this case.

        return null;
    }

    private string? GetPinyinErrorText()
    {
        if (string.IsNullOrEmpty(Pinyin))
        {
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!AsciiLettersRegex().IsMatch(Pinyin))
        {
            return Localizer[Resource.OnlyAsciiAllowed];
        }

        return null;
    }

    private string? GetIpaErrorText()
    {
        if (string.IsNullOrEmpty(Ipa))
        {
            return Localizer[Resource.CompulsoryValue];
        }
        else if (!TextUtils.IsOnlyIpaCharacters(Ipa))
        {
            return Localizer[Resource.OnlyIpaAllowed];
        }

        return null;
    }

    private string? GetToneErrorText()
    {
        if (Tone is null)
        {
            return Localizer[Resource.CompulsoryValue];
        }

        // Null tone is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.

        return null;
    }

    private string? GetStrokesErrorText()
    {
        if (Strokes is null)
        {
            return Localizer[Resource.CompulsoryValue];
        }

        // Null strokes is the only reachable wrong input.
        // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.

        return null;
    }
}
