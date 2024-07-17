using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeFormBase : ComponentBase, IEntityForm
{
    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected string? OriginalCharacter { get; set; }

    protected string? OriginalPinyin { get; set; }

    protected byte? OriginalTone { get; set; }

    public byte? Strokes { get; set; }

    public string? TheCharacter { get; set; }

    public string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

    public string BackdropId { get; } = IDs.INDEX_BACKDROP;

    public string HtmlTitleOnClose { get; set; } = default!;

    public IModal? LowerLevelModal { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IIndex Index { get; set; } = default!;

    public async Task OpenAsync(string htmlTitleOnClose, CancellationToken cancellationToken) =>
        await ModalCommons.OpenAsyncCommon(
            this,
            Localizer[Resource.CreateNewAlternative],
            htmlTitleOnClose,
            cancellationToken
        );

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            ClearWrongInputAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, cancellationToken),
            JSInteropDOM.SetZIndexAsync(IDs.ALTERNATIVE_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            OriginalSelector.OpenAsync(this, Localizer[Resource.CreateNewAlternative], cancellationToken)
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
        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.ALTERNATIVE_FORM_STROKES_INPUT, IDs.ALTERNATIVE_FORM_STROKES_ERROR, GetStrokesErrorText),
            (IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, GetOriginalErrorText)
        );

        if (areAllInputsValid)
        {
            // TODO submit
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

    // Null strokes is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetStrokesErrorText() =>
        EntityFormCommons.GetNullInputErrorText(Strokes);

    private string? GetOriginalErrorText() =>
        EntityFormCommons.GetNullInputErrorText(OriginalCharacter);
}
