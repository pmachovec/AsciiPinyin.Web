using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Validation;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ChacharFormBase : ComponentBase, IEntityForm<Chachar>
{
    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string ClearAlternativeClasses { get; private set; } = string.Empty;

    protected EntitySelector<Alternative> AlternativeSelector { get; set; } = default!;

    protected EntitySelector<Chachar> RadicalSelector { get; set; } = default!;

    protected IEnumerable<Alternative> AvailableAlternatives = [];

    protected Chachar Chachar { get; set; } = new();

    protected EditContext EditContext = default!;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IBackdrop? Backdrop { get; set; }

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.CHACHAR_FORM_ROOT;

    public int ZIndex { get; set; }

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

    protected override async Task OnAfterRenderAsync(bool firstRender) => await SetClearAlternativeButtonAsync(CancellationToken.None);

    public async Task OpenAsync(Chachar chachar, IModal modalLowerLevel, CancellationToken cancellationToken)
    {
        Chachar = chachar;

        AvailableAlternatives = Chachar.IsRadical
            ? []
            : Index.Alternatives.Where(alternative =>
                alternative.OriginalCharacter == Chachar.RadicalCharacter
                && alternative.OriginalPinyin == Chachar.RadicalPinyin
                && alternative.OriginalTone == Chachar.RadicalTone
            );

        ModalLowerLevel = modalLowerLevel;
        Page = null;
        SetUpEditContext();
        await ModalCommons.OpenHigherLevelAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task OpenAsync(Chachar chachar, IPage page, CancellationToken cancellationToken)
    {
        Chachar = chachar;

        AvailableAlternatives = Chachar.IsRadical
            ? []
            : Index.Alternatives.Where(alternative =>
                alternative.OriginalCharacter == Chachar.RadicalCharacter
                && alternative.OriginalPinyin == Chachar.RadicalPinyin
                && alternative.OriginalTone == Chachar.RadicalTone
            );

        ModalLowerLevel = null;
        Page = page;
        SetUpEditContext();
        await ModalCommons.OpenFirstLevelAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        Chachar = new();
        AvailableAlternatives = [];
        ModalLowerLevel = null;
        Page = page;
        SetUpEditContext();
        await ModalCommons.OpenFirstLevelAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

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

        await StateHasChangedAsync();

        await Task.WhenAll(
            SetClearAlternativeButtonAsync(cancellationToken),
            RadicalSelector.CloseAsync(cancellationToken)
        );
    }

    protected async Task SelectAlternativeAsync(Alternative alternative, CancellationToken cancellationToken)
    {
        Chachar.RadicalAlternativeCharacter = alternative.TheCharacter;
        StateHasChanged();
        await AlternativeSelector.CloseAsync(cancellationToken);
    }

    protected async Task ClearRadicalAsync(CancellationToken cancellationToken)
    {
        Chachar.RadicalCharacter = null;
        Chachar.RadicalPinyin = null;
        Chachar.RadicalTone = null;
        Chachar.RadicalAlternativeCharacter = null;
        AvailableAlternatives = [];
        await DisableClearAlternativeButtonAsync(cancellationToken);
        StateHasChanged();
    }

    protected void ClearAlternative()
    {
        Chachar.RadicalAlternativeCharacter = null;
        StateHasChanged();
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        if (
            changeEventArgs.Value is string theCharacter
            && theCharacter.Length > 1
            && TextUtils.GetStringRealLength(theCharacter) > 1
        )
        {
            var theCharacterStart = TextUtils.GetStringFirstCharacterAsString(theCharacter);
            Chachar.TheCharacter = theCharacterStart;
            await JSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_THE_CHARACTER_INPUT, theCharacterStart, cancellationToken);
        }
    }

    protected async Task PreventToneInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        var correctTone = await EntityFormCommons.GetCorrectNumberInputValueAsync(
            IDs.CHACHAR_FORM_TONE_INPUT,
            changeEventArgs.Value,
            Chachar.Tone,
            cancellationToken
        );

        Chachar.Tone = correctTone;

        await JSInteropDOM.SetValueAsync(
            IDs.CHACHAR_FORM_TONE_INPUT,
            correctTone,
            cancellationToken
        );
    }

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        var correctStrokes = await EntityFormCommons.GetCorrectNumberInputValueAsync(
            IDs.CHACHAR_FORM_STROKES_INPUT,
            changeEventArgs.Value,
            Chachar.Strokes,
            cancellationToken
        );

        Chachar.Strokes = correctStrokes;

        await JSInteropDOM.SetValueAsync(
            IDs.CHACHAR_FORM_STROKES_INPUT,
            correctStrokes,
            cancellationToken
        );
    }

    protected void ClearError(string fieldName)
    {
        var fieldIdentifier = new FieldIdentifier(Chachar, fieldName);
        EditContext.NotifyFieldChanged(fieldIdentifier);
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        await Index.ProcessDialog.SetProcessingAsync(this, cancellationToken);
        LogCommons.LogFormSubmittedDebug(Logger, nameof(Chachar));
        LogCommons.LogFormDataDebug(Logger, nameof(Chachar), Chachar);
        var databseIntegrityErrorText = GetDatabaseIntegrityErrorText(Chachar);

        if (databseIntegrityErrorText is not null)
        {
            await Index.ProcessDialog.SetErrorAsync(
                this,
                databseIntegrityErrorText,
                cancellationToken
            );
        }
        else
        {
            await ModalCommons.PostAsync(
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
        }
    }

    private void SetUpEditContext()
    {
        EditContext = new(Chachar);
        _ = new FormValidationHandler<ChacharForm, Chachar>(Logger, Localizer, EditContext);
    }

    private async Task SetClearAlternativeButtonAsync(CancellationToken cancellationToken)
    {
        if (AvailableAlternatives.Any())
        {
            await EnableClearAlternativeButtonAsync(cancellationToken);
        }
        else
        {
            await DisableClearAlternativeButtonAsync(cancellationToken);
        }
    }

    private async Task EnableClearAlternativeButtonAsync(CancellationToken cancellationToken)
    {
        ClearAlternativeClasses = CssClasses.BTN_OUTLINE_PRIMARY;

        await Task.WhenAll(
            JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_ALTERNATIVE_INPUT, cancellationToken),
            JSInteropDOM.EnableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, cancellationToken)
        );
    }

    private async Task DisableClearAlternativeButtonAsync(CancellationToken cancellationToken)
    {
        ClearAlternativeClasses = CssClasses.BTN_OUTLINE_SECONDARY;

        await Task.WhenAll(
            JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_ALTERNATIVE_INPUT, cancellationToken),
            JSInteropDOM.DisableAsync(IDs.CHACHAR_FORM_CLEAR_ALTERNATIVE, cancellationToken)
        );
    }

    private string? GetDatabaseIntegrityErrorText(Chachar chachar)
    {
        if (Index.Chachars.Contains(chachar))
        {
            LogCommons.LogChacharAlreadyExistsError(Logger);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.CharacterAlreadyExists],
                chachar.TheCharacter,
                chachar.RealPinyin
            );
        }

        return null;
    }
}
