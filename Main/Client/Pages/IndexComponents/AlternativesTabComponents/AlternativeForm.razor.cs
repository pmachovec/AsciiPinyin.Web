using AsciiPinyin.Web.Client.AbstractComponentBases;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeFormBase : EntityFormBase
{
    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected string? OriginalCharacter { get; set; }

    protected string? OriginalPinyin { get; set; }

    protected byte? OriginalTone { get; set; }

    public override byte? Strokes { get; set; }

    public override string? TheCharacter { get; set; }

    public override string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

    public override event EventHandler EventOnClose = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IModalCommons ModalWithBackdropCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required Index Index { get; set; } = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            OriginalSelector.EventOnClose += async (_, _) =>
            {
                await Task.WhenAll(
                    JSInteropDOM.SetTitleAsync(Localizer[Resource.CreateNewAlternative], CancellationToken.None),
                    JSInteropDOM.SetZIndexAsync(IDs.ALTERNATIVE_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z + 1, CancellationToken.None));
            };
        }
    }

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.OpenAsyncCommon(
            this,
            Localizer[Resource.CreateNewAlternative],
            cancellationToken);
    }

    public override async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalWithBackdropCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);
    }

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.ALTERNATIVE_FORM_ROOT, ByteConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            OriginalSelector.OpenAsync(cancellationToken));
    }

    protected async Task SelectOriginalAsync(Chachar originalChachar, CancellationToken cancellationToken)
    {
        OriginalCharacter = originalChachar.TheCharacter;
        OriginalPinyin = originalChachar.Pinyin;
        OriginalTone = originalChachar.Tone;
        StateHasChanged();
        await OriginalSelector.CloseAsync(cancellationToken);
    }

    protected void ClearOriginal()
    {
        OriginalCharacter = null;
        OriginalPinyin = null;
        OriginalTone = null;
        StateHasChanged();
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        await EntityFormCommons.PreventMultipleCharactersAsync(
            this,
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            changeEventArgs,
            cancellationToken);
    }

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventStrokesInvalidAsync(this, IDs.ALTERNATIVE_FORM_STROKES_INPUT, changeEventArgs, cancellationToken);

    protected async Task ClearWrongInputAsync(string inputId, string errorId, CancellationToken cancellationToken) =>
        await EntityFormCommons.ClearWrongInputAsync(inputId, errorId, cancellationToken);

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.ALTERNATIVE_FORM_STROKES_INPUT, IDs.ALTERNATIVE_FORM_STROKES_ERROR, GetStrokesErrorText));

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
