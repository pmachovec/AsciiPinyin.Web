using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharFormBase : ModalWithBackdropBaseGeneral
{
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

    protected async Task PreventTooLongCharacterInputAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        if (changeEventArgs.Value is null)
        {
            TheCharacter = null;
            await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, string.Empty, cancellationToken);
            return;
        }

        if (
            changeEventArgs.Value is string theCharacter
            && (theCharacter.Length <= 1 || TextUtils.GetStringRealLength(theCharacter) <= 1))
        {
            TheCharacter = theCharacter;
        }
        else
        {
            await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, TheCharacter ?? string.Empty, cancellationToken);
        }
    }

    protected async Task ClearWrongInputAsync(string inputId, string errorId, CancellationToken cancellationToken)
    {
        await JSInteropDOM.RemoveClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken);
        await JSInteropDOM.RemoveTextAsync(errorId, cancellationToken);
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        var success = await CheckTheCharacter(cancellationToken);
        // TODO && CheckPinyin && CheckStrokes etc.

        if (success)
        {
            // TODO submit
        }
    }

    private async Task<bool> CheckTheCharacter(CancellationToken cancellationToken)
    {
        var theCharacterErrorText = GetTheCharacterErrorText();

        if (theCharacterErrorText is { } yesThereIsTheCharacterErrorText)
        {
            await JSInteropDOM.AddClassAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, CssClasses.BORDER_DANGER, cancellationToken);
            await JSInteropDOM.SetTextAsync(IDs.CHACHAR_FORM_THE_CHARACTER_ERROR, yesThereIsTheCharacterErrorText, cancellationToken);
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
        else if (TextUtils.GetStringRealLength(TheCharacter) > 1)
        {
            return Localizer[Resource.OnlyOneCharacterAllowed];
        }
        else if (!TextUtils.IsOnlyChineseCharacters(TheCharacter))
        {
            return Localizer[Resource.MustBeChineseCharacter];
        }

        return null;
    }
}
