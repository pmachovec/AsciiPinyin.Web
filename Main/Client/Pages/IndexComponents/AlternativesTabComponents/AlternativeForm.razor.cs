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

    public string HtmlTitle { get; private set; } = string.Empty;

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

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

    protected override void OnInitialized() =>
        HtmlTitle = Localizer[Resource.CreateNewAlternative];

    public async Task OpenAsync(CancellationToken cancellationToken) =>
        await ModalCommons.OpenAsyncCommon(
            this,
            HtmlTitle,
            cancellationToken
        );

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
        var areAllInputsValid = await EntityFormCommons.CheckInputsAsync(
            cancellationToken,
            (IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, IDs.ALTERNATIVE_FORM_THE_CHARACTER_ERROR, GetTheCharacterErrorText),
            (IDs.ALTERNATIVE_FORM_STROKES_INPUT, IDs.ALTERNATIVE_FORM_STROKES_ERROR, GetStrokesErrorText),
            (IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, IDs.ALTERNATIVE_FORM_ORIGINAL_ERROR, GetOriginalErrorText)
        );

        if (areAllInputsValid)
        {
            var alternative = new Alternative()
            {
                OriginalCharacter = OriginalCharacter,
                OriginalPinyin = OriginalPinyin,
                OriginalTone = OriginalTone,
                Strokes = Strokes,
                TheCharacter = TheCharacter
            };

            if (Index.Alternatives.Contains(alternative))
            {
                await Index.FormSubmit.SetErrorAsync(
                    this,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Localizer[Resource.AlternativeAlreadyInDb],
                        alternative.TheCharacter,
                        alternative.OriginalCharacter,
                        alternative.OriginalPinyin
                    ),
                    cancellationToken
                );
            }
            else
            {
                var postTask = EntityClient.PostEntityAsync(ApiNames.ALTERNATIVES, alternative, cancellationToken);

                await Index.FormSubmit.SetProcessingAsync(
                    this,
                    postTask,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Localizer[Resource.AlternativeCreated],
                        alternative.TheCharacter,
                        alternative.OriginalCharacter,
                        alternative.OriginalPinyin
                    ),
                    Localizer[Resource.ProcessingError],
                    cancellationToken
                );
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

    // Null strokes is the only reachable wrong input.
    // Invalid inputs are unreachable thanks to PreventToneInvalidAsync, no need to handle this case.
    private string? GetStrokesErrorText() =>
        EntityFormCommons.GetNullInputErrorText(Strokes);

    private string? GetOriginalErrorText() =>
        EntityFormCommons.GetNullInputErrorText(OriginalCharacter);
}
