using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public partial class ChacharFormBase : ModalWithBackdropBaseGeneral
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

    protected byte? Strokes { get; set; }

    protected string? TheCharacter { get; set; }

    protected byte? Tone { get; set; }

    public override string BackdropId { get; } = IDs.CHACHAR_FORM_BACKDROP;

    public override string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

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
        await this.OpenAsyncExtension(
            JSInteropDOM,
            Localizer[Resource.CreateNewCharacter],
            cancellationToken);
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await this.CloseAsyncExtension(
            JSInteropDOM,
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

    protected async Task PreventMultipleCharacters(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        if (changeEventArgs.Value is null)
        {
            TheCharacter = null;
            await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, string.Empty, cancellationToken);
            return;
        }
        else if (changeEventArgs.Value is string theCharacter)
        {
            if (theCharacter.Length <= 1 || TextUtils.GetStringRealLength(theCharacter) <= 1)
            {
                TheCharacter = theCharacter;
            }
            else
            {
                var theCharacterStart = TextUtils.GetStringFirstCharacterAsString(theCharacter);
                TheCharacter = theCharacterStart;
                await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, theCharacterStart, cancellationToken);
            }
        }
    }

    protected async Task ClearWrongInputAsync(string inputId, string errorId, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.RemoveClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
            JSInteropDOM.RemoveTextAsync(errorId, cancellationToken));
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        var separateCheckSuccesses = await Task.WhenAll(
            CheckTheCharacter(cancellationToken),
            CheckPinyin(cancellationToken));
        // TODO && CheckPinyin && CheckStrokes etc.

        var totalSuccess = separateCheckSuccesses.All(success => success);

        if (totalSuccess)
        {
            // TODO submit
        }
    }

    private async Task<bool> CheckTheCharacter(CancellationToken cancellationToken) =>
        await CheckInput(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, IDs.CHACHAR_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText, cancellationToken);

    private async Task<bool> CheckPinyin(CancellationToken cancellationToken) =>
        await CheckInput(IDs.CHACHAR_FORM_PINYIN_INPUT, IDs.CHACHAR_FORM_PINYIN_ERROR, GetPinyinErrorText, cancellationToken);

    private async Task<bool> CheckInput(
        string inputId,
        string errorDivId,
        Func<string?> getErrorText,
        CancellationToken cancellationToken)
    {
        if (getErrorText() is { } errorText)
        {
            await Task.WhenAll(
                JSInteropDOM.AddClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
                JSInteropDOM.SetTextAsync(errorDivId, errorText, cancellationToken));
            return false;
        }

        return true;
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

        // Multi-character input is unreachable thanks to PreventMultipleCharacters, no need to handle this case.

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
}
