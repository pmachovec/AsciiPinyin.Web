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

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeFormBase : ComponentBase, IEntityForm<Alternative>
{
    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string OriginalSelectorClasses { get; private set; } = string.Empty;

    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected Alternative Alternative { get; set; } = new();

    protected EditContext EditContext = default!;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IEntityFormCommons EntityFormCommons { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private ILogger<AlternativeForm> Logger { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required IIndex Index { get; init; }

    protected override void OnInitialized()
    {
        HtmlTitle = Localizer[Resource.CreateNewAlternative];
        SetUpEditContext();
        EditContext.OnValidationRequested += (_, _) => ValidateAdditional();
    }

    public async Task OpenAsync(Alternative alternative, IPage page, CancellationToken cancellationToken)
    {
        Alternative = alternative;
        await OpenAsync(page, cancellationToken);
    }

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        ModalLowerLevel = null;
        Page = page;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken)
    {
        ClearOriginal();

        await Task.WhenAll(
            JSInteropDOM.SetZIndexAsync(
                IDs.ALTERNATIVE_FORM_ROOT,
                NumberConstants.INDEX_BACKDROP_Z - 1,
                cancellationToken
            ),
            OriginalSelector.OpenAsync(
                this,
                cancellationToken
            )
        );
    }

    protected async Task SelectOriginalAsync(Chachar originalChachar, CancellationToken cancellationToken)
    {
        Alternative.OriginalCharacter = originalChachar.TheCharacter;
        Alternative.OriginalPinyin = originalChachar.Pinyin;
        Alternative.OriginalTone = originalChachar.Tone;
        StateHasChanged();
        await OriginalSelector.CloseAsync(cancellationToken);
    }

    protected void ClearOriginal()
    {
        ClearError(nameof(Alternative.OriginalCharacter));
        OriginalSelectorClasses = string.Empty;
        Alternative.OriginalCharacter = null;
        Alternative.OriginalPinyin = null;
        Alternative.OriginalTone = null;
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
            Alternative.TheCharacter = theCharacterStart;
            await JSInteropDOM.SetValueAsync(IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT, theCharacterStart, cancellationToken);
        }
    }

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken)
    {
        var correctStrokes = await EntityFormCommons.GetCorrectNumberInputValueAsync(
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            changeEventArgs.Value,
            Alternative.Strokes,
            cancellationToken
        );

        Alternative.Strokes = correctStrokes;

        await JSInteropDOM.SetValueAsync(
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            correctStrokes,
            cancellationToken
        );
    }

    protected void ClearError(string fieldName)
    {
        var fieldIdentifier = new FieldIdentifier(Alternative, fieldName);
        EditContext.NotifyFieldChanged(fieldIdentifier);
    }

    protected void ValidateAdditional()
    {
        var originalField = new FieldIdentifier(Alternative, nameof(Alternative.OriginalCharacter));

        if (EditContext.GetValidationMessages(originalField).Any())
        {
            OriginalSelectorClasses = CssClasses.INVALID;
        }
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        await Index.ProcessDialog.SetProcessingAsync(this, cancellationToken);
        LogCommons.LogFormSubmittedDebug(Logger, nameof(Alternative));
        LogCommons.LogFormDataDebug(Logger, nameof(Alternative), Alternative);
        var databseIntegrityErrorText = GetDatabaseIntegrityErrorText(Alternative);

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
            var isPostSuccessful = await ModalCommons.PostAsync(
                this,
                Alternative,
                Index,
                EntityClient.PostEntityAsync,
                ApiNames.ALTERNATIVES,
                Logger,
                Index.Alternatives.Add,
                Resource.AlternativeCreated,
                cancellationToken,
                Alternative.TheCharacter!,
                Alternative.OriginalCharacter!,
                Alternative.OriginalRealPinyin!
            );

            if (isPostSuccessful)
            {
                ClearForm();
            }
        }
    }

    private string? GetDatabaseIntegrityErrorText(Alternative alternative)
    {
        if (Index.Alternatives.Contains(alternative))
        {
            LogCommons.LogAlternativeAlreadyExistsError(Logger);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.AlternativeAlreadyInDb],
                alternative.TheCharacter,
                alternative.OriginalCharacter,
                alternative.OriginalRealPinyin
            );
        }

        return null;
    }

    private void ClearForm()
    {
        Alternative = new();
        SetUpEditContext();
    }

    private void SetUpEditContext()
    {
        EditContext = new(Alternative);
        _ = new FormValidationHandler<AlternativeForm, Alternative>(Logger, Localizer, EditContext);
    }
}
