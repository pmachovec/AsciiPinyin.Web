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
using System.Net;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public partial class ChacharFormBase : ComponentBase, IEntityForm
{
    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex AsciiLettersRegex();

    protected EntitySelector<Alternative> AlternativeSelector { get; set; } = default!;

    protected EntitySelector<Chachar> RadicalSelector { get; set; } = default!;

    protected SaveFailed SaveFailed { get; set; } = default!;

    protected SaveSuccess SaveSuccess { get; set; } = default!;

    protected IEnumerable<Alternative> AvailableAlternatives = [];

    protected string? Ipa { get; set; }

    protected string? Pinyin { get; set; }

    protected string? RadicalAlternativeCharacter { get; set; }

    protected string? RadicalCharacter { get; set; }

    protected string? RadicalPinyin { get; set; }

    protected byte? RadicalTone { get; set; }

    protected byte? Tone { get; set; }

    public byte? Strokes { get; set; }

    public string? TheCharacter { get; set; }

    public string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

    public string BackdropId { get; } = IDs.INDEX_BACKDROP;

    public string HtmlTitleOnClose { get; set; } = default!;

    public IModal? LowerLevelModal { get; set; }

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IIndex Index { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // No need to set these properties for these elements explicitly in the HTML part.
        if (AvailableAlternatives.Any())
        {
            await Task.WhenAll(
                JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_ALTERNATIVE, CancellationToken.None),
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
                JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_ALTERNATIVE, CancellationToken.None),
                JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_DARK, CancellationToken.None),
                JSInteropDOM.RemoveClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_PRIMARY, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_ALTERNATIVE_LABEL, CssClasses.TEXT_BLACK_50, CancellationToken.None),
                JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, CssClasses.BTN_OUTLINE_SECONDARY, CancellationToken.None)
            );
        }
    }

    public async Task OpenAsync(string htmlTitleOnClose, CancellationToken cancellationToken) =>
        await ModalCommons.OpenAsyncCommon(
            this,
            Localizer[Resource.CreateNewCharacter],
            htmlTitleOnClose,
            cancellationToken
        );

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    protected async Task OpenRadicalSelectorAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            RadicalSelector.OpenAsync(this, Localizer[Resource.CreateNewCharacter], cancellationToken)
        );
    }

    protected async Task OpenAlternativeSelectorAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            AlternativeSelector.OpenAsync(this, Localizer[Resource.CreateNewCharacter], cancellationToken)
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

            var statusCode = await EntityClient.PostEntityAsync(ApiNames.CHARACTERS, chachar, cancellationToken);

            if (statusCode == HttpStatusCode.OK)
            {
                await Index.SaveSuccess.OpenAsync(this, Localizer[Resource.CreateNewCharacter], cancellationToken);
            }
            else
            {
                await Index.SaveFailed.OpenAsync(this, Localizer[Resource.CreateNewCharacter], cancellationToken);
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

    // Null tone is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetToneErrorText() =>
        EntityFormCommons.GetNullInputErrorText(Tone);

    // Null strokes is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetStrokesErrorText() =>
        EntityFormCommons.GetNullInputErrorText(Strokes);
}
