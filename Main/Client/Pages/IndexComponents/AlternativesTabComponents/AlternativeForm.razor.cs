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

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeFormBase : ComponentBase, IEntityForm
{
    protected EntitySelector<Chachar> OriginalSelector { get; set; } = default!;

    protected Alternative Alternative { get; set; } = new();

    protected EditContext EditContext = default!;

    public string RootId { get; } = IDs.ALTERNATIVE_FORM_ROOT;

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
        EditContext.OnValidationRequested += async (_, _) => await ValidateAdditionalAsync(CancellationToken.None);
    }

    public async Task OpenAsync(IPage page, CancellationToken cancellationToken)
    {
        ModalLowerLevel = null;
        Page = page;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    protected async Task OpenOriginalSelectorAsync(CancellationToken cancellationToken) =>
        await Task.WhenAll(
            ClearOriginalAsync(cancellationToken),
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

    protected async Task SelectOriginalAsync(Chachar originalChachar, CancellationToken cancellationToken)
    {
        Alternative.OriginalCharacter = originalChachar.TheCharacter;
        Alternative.OriginalPinyin = originalChachar.Pinyin;
        Alternative.OriginalTone = originalChachar.Tone;
        StateHasChanged();
        await OriginalSelector.CloseAsync(cancellationToken);
    }

    protected async Task ClearOriginalAsync(CancellationToken cancellationToken)
    {
        ClearError(nameof(Alternative.OriginalCharacter));

        await Task.WhenAll(
            JSInteropDOM.RemoveClassAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, CssClasses.INVALID, cancellationToken),
            JSInteropDOM.RemoveClassAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_CLEAR, CssClasses.INVALID, cancellationToken)
        );

        Alternative.OriginalCharacter = null;
        Alternative.OriginalPinyin = null;
        Alternative.OriginalTone = null;
    }

    protected async Task PreventMultipleCharactersAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventMultipleCharactersAsync(
            IDs.ALTERNATIVE_FORM_THE_CHARACTER_INPUT,
            changeEventArgs,
            cancellationToken
        );

    protected async Task PreventStrokesInvalidAsync(ChangeEventArgs changeEventArgs, CancellationToken cancellationToken) =>
        await EntityFormCommons.PreventStrokesInvalidAsync(
            IDs.ALTERNATIVE_FORM_STROKES_INPUT,
            changeEventArgs,
            Alternative.Strokes,
            cancellationToken
        );

    protected void ClearError(string fieldName)
    {
        var fieldIdentifier = new FieldIdentifier(Alternative, fieldName);
        EditContext.NotifyFieldChanged(fieldIdentifier);
    }

    protected async Task ValidateAdditionalAsync(CancellationToken cancellationToken)
    {
        var originalFiled = new FieldIdentifier(Alternative, nameof(Alternative.OriginalCharacter));

        if (EditContext.GetValidationMessages(originalFiled).Any())
        {
            await Task.WhenAll(
                JSInteropDOM.AddClassAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_INPUT, CssClasses.INVALID, cancellationToken),
                JSInteropDOM.AddClassAsync(IDs.ALTERNATIVE_FORM_ORIGINAL_CLEAR, CssClasses.INVALID, cancellationToken)
            );
        }
    }

    protected async Task CheckAndSubmitAsync(CancellationToken cancellationToken)
    {
        await Index.SubmitDialog.SetProcessingAsync(this, cancellationToken);
        LogCommons.LogFormSubmittedDebug(Logger, nameof(Alternative));
        LogCommons.LogFormDataDebug(Logger, nameof(Alternative), Alternative);
        LogCommons.LogDatabaseIntegrityVerificationDebug(Logger);
        var databseIntegrityErrorText = GetDatabaseIntegrityErrorText(Alternative);

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
        _ = new FormValidationHandler<AlternativeForm>(Logger, Localizer, EditContext);
    }
}
