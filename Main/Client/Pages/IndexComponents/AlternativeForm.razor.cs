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

public class AlternativeFormBase : ComponentBase, IEntityForm<Alternative>
{
    private Alternative? _oldAlternative;
    private Func<CancellationToken, Task> _submitAsync = default!;

    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string OriginalSelectorClasses { get; private set; } = string.Empty;

    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected Alternative Alternative { get; set; } = new();

    protected EditContext EditContext = default!;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IBackdrop? Backdrop { get; set; }

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

    public int ZIndex { get; set; }

    public Func<CancellationToken, Task> CloseAsync { get; private set; } = default!;

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
    }

    public async Task OpenAsync(Alternative entity, IModal modalLowerLevel, CancellationToken cancellationToken)
    {
        Alternative = entity;
        _oldAlternative = new Alternative(entity);
        _submitAsync = PutAsync;
        ModalLowerLevel = modalLowerLevel;
        CloseAsync = async (cancellationToken) => await ModalCommons.CloseHigherLevelAsyncCommon(this, cancellationToken);
        Page = null;
        SetUpEditContext();
        await ModalCommons.OpenHigherLevelAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        Alternative = new();
        _submitAsync = PostAsync;
        ModalLowerLevel = null;
        CloseAsync = async (cancellationToken) => await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);
        Page = page;
        OriginalSelectorClasses = string.Empty;
        SetUpEditContext();
        await ModalCommons.OpenFirstLevelAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken)
    {
        ClearOriginal();
        await OriginalSelector.OpenAsync(this, cancellationToken);
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
            await _submitAsync(cancellationToken);
        }
    }

    private async Task PostAsync(CancellationToken cancellationToken)
    {
        var isSuccess = await ModalCommons.SubmitAsync(
            this,
            Alternative,
            Index,
            EntityClient.PostEntityAsync,
            HttpMethod.Post,
            ApiNames.ALTERNATIVES,
            Logger,
            Resource.AlternativeCreated,
            cancellationToken,
            Alternative.TheCharacter!,
            Alternative.OriginalCharacter!,
            Alternative.OriginalRealPinyin!
        );

        if (isSuccess)
        {
            _ = Index.Alternatives.Add(Alternative);
            await Index.StateHasChangedAsync();
        }
    }

    private async Task PutAsync(CancellationToken cancellationToken)
    {
        var isSuccess = await ModalCommons.SubmitAsync(
            this,
            Alternative,
            Index,
            EntityClient.PutEntityAsync,
            HttpMethod.Put,
            ApiNames.ALTERNATIVES,
            Logger,
            Resource.AlternativeChanged,
            cancellationToken,
            Alternative.TheCharacter!,
            Alternative.OriginalCharacter!,
            Alternative.OriginalRealPinyin!
        );

        if (isSuccess)
        {
            if (_oldAlternative is not null)
            {
                _ = Index.Alternatives.Remove(_oldAlternative);
            }

            _ = Index.Alternatives.Add(Alternative);
            await Index.StateHasChangedAsync();
        }
    }

    private string? GetDatabaseIntegrityErrorText(Alternative alternative)
    {
        if (Index.Alternatives.Contains(alternative))
        {
            LogCommons.LogAlternativeAlreadyExistsError(Logger);

            return string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.AlternativeAlreadyExists],
                alternative.TheCharacter,
                alternative.OriginalCharacter,
                alternative.OriginalRealPinyin
            );
        }

        return null;
    }

    private void SetUpEditContext()
    {
        EditContext = new(Alternative);
        _ = new FormValidationHandler<AlternativeForm, Alternative>(Logger, Localizer, EditContext);
        EditContext.OnValidationRequested += (_, _) => ValidateAdditional();
    }

    private void ValidateAdditional()
    {
        var originalField = new FieldIdentifier(Alternative, nameof(Alternative.OriginalCharacter));

        if (EditContext.GetValidationMessages(originalField).Any())
        {
            OriginalSelectorClasses = CssClasses.INVALID;
        }
    }
}
