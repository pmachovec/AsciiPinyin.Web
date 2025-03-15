using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Validation;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharFormBase : ComponentBase, IEntityForm
{
    protected EntitySelector<Alternative> AlternativeSelector { get; set; } = default!;

    protected EntitySelector<Chachar> RadicalSelector { get; set; } = default!;

    protected IEnumerable<Alternative> AvailableAlternatives = [];

    protected Chachar Chachar { get; set; } = new();

    protected EditContext EditContext = default!;

    public string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    public string HtmlTitle { get; private set; } = string.Empty;

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

    protected override void OnInitialized()
    {
        HtmlTitle = Localizer[Resource.CreateNewCharacter];
        SetUpEditContext();
    }

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

    protected async Task OpenRadicalSelectorAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, NumberConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            RadicalSelector.OpenAsync(this, cancellationToken)
        );

    protected async Task OpenAlternativeSelectorAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(IDs.CHACHAR_FORM_ROOT, NumberConstants.INDEX_BACKDROP_Z - 1, cancellationToken),
            AlternativeSelector.OpenAsync(this, cancellationToken)
        );

    protected async Task SelectRadicalAsync(Chachar radicalChachar, CancellationToken cancellationToken)
    {
        Chachar.RadicalCharacter = radicalChachar.TheCharacter;
        Chachar.RadicalPinyin = radicalChachar.Pinyin;
        Chachar.RadicalTone = radicalChachar.Tone;
        Chachar.RadicalAlternativeCharacter = null;

        AvailableAlternatives = Index.Alternatives.Where(alternative =>
            alternative.OriginalCharacter == radicalChachar.TheCharacter
            && alternative.OriginalPinyin == radicalChachar.Pinyin
            && alternative.OriginalTone == radicalChachar.Tone
        );

        StateHasChanged();
        await RadicalSelector.CloseAsync(cancellationToken);
    }

    protected async Task SelectAlternativeAsync(Alternative alternative, CancellationToken cancellationToken)
    {
        Chachar.RadicalAlternativeCharacter = alternative.TheCharacter;
        StateHasChanged();
        await AlternativeSelector.CloseAsync(cancellationToken);
    }

    protected void ClearRadical()
    {
        Chachar.RadicalCharacter = null;
        Chachar.RadicalPinyin = null;
        Chachar.RadicalTone = null;
        Chachar.RadicalAlternativeCharacter = null;
        AvailableAlternatives = [];
        StateHasChanged();
    }

    protected void ClearAlternative()
    {
        Chachar.RadicalAlternativeCharacter = null;
        StateHasChanged();
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventMultipleCharactersAsync(
            IDs.CHACHAR_FORM_THE_CHARACTER_INPUT,
            changeEventArgs,
            cancellationToken
        );

    protected async Task PreventToneInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        var correctTone = await EntityFormCommons.GetCorrectNumberInputValueAsync(
            IDs.CHACHAR_FORM_TONE_INPUT,
            changeEventArgs.Value,
            Chachar.Tone,
            cancellationToken
        );

        await JSInteropDOM.SetValueAsync(
            IDs.CHACHAR_FORM_TONE_INPUT,
            correctTone?.ToString(CultureInfo.InvariantCulture)!,
            cancellationToken
        );
    }

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventStrokesInvalidAsync(
            IDs.CHACHAR_FORM_STROKES_INPUT,
            changeEventArgs,
            Chachar.Strokes,
            cancellationToken
        );

    protected void ClearError(string fieldName)
    {
        var fieldIdentifier = new FieldIdentifier(Chachar, fieldName);
        EditContext.NotifyFieldChanged(fieldIdentifier);
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        await Index.SubmitDialog.SetProcessingAsync(this, cancellationToken);
        LogCommons.LogFormSubmittedDebug(Logger, nameof(Chachar));
        LogCommons.LogFormDataDebug(Logger, nameof(Chachar), Chachar);
        var databseIntegrityErrorText = GetDatabaseIntegrityErrorText(Chachar);

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
            var isPostSuccessful = await ModalCommons.PostAsync(
                this,
                Chachar,
                Index,
                EntityClient.PostEntityAsync,
                ApiNames.CHARACTERS,
                Logger,
                Index.Chachars.Add,
                Resource.CharacterCreated,
                cancellationToken,
                Chachar.TheCharacter!,
                Chachar.RealPinyin!
            );

            if (isPostSuccessful)
            {
                ClearForm();
            }
        }
    }

    private string? GetDatabaseIntegrityErrorText(Chachar chachar)
    {
        if (Index.Chachars.Contains(chachar))
        {
            LogCommons.LogChacharAlreadyExistsError(Logger);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.CharacterAlreadyInDb],
                chachar.TheCharacter,
                chachar.RealPinyin
            );
        }

        return null;
    }

    private void ClearForm()
    {
        Chachar = new();
        SetUpEditContext();
    }

    private void SetUpEditContext()
    {
        EditContext = new(Chachar);
        _ = new FormValidationHandler<ChacharForm>(Logger, Localizer, EditContext);
    }
}
