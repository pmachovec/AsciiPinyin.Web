using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ChacharViewDialogBase : ComponentBase, IEntityViewDialog<Chachar>
{
    protected Chachar? Chachar { get; set; }

    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string DisableDeleteCss { get; private set; } = string.Empty;

    protected string DeleteTitle { get; private set; } = string.Empty;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IBackdrop? Backdrop { get; set; }

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.CHACHAR_VIEW_DIALOG_ROOT;

    public int ZIndex { get; set; }

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private ILogger<ChacharViewDialog> Logger { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public required IIndex Index { get; init; }

    public async Task OpenAsync(
        Chachar entity,
        IPage page,
        CancellationToken cancellationToken
    )
    {
        Page = page;
        ModalLowerLevel = null;
        await Index.ProcessDialog.SetProcessingAsync(page, cancellationToken);
        await OpenAsyncCommon(entity, cancellationToken);
        await ModalCommons.OpenFirstLevelAsyncCommon(this, HtmlTitle, cancellationToken);
        await Index.ProcessDialog.CloseWithoutBackdropAsync(cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        Chachar = null;
        await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);
    }

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected async Task InitiateDeleteAsync(CancellationToken cancellationToken) =>
        await Index.ProcessDialog.SetWarningAsync(
            this,
            string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.CharacterWillBeDeleted],
                Chachar!.TheCharacter!,
                Chachar.RealPinyin!
            ),
            SubmitDeleteAsync,
            cancellationToken
        );

    protected async Task OpenFormAsync(CancellationToken cancellationToken) =>
        await Index.ChacharForm.OpenAsync(Chachar!, this, cancellationToken);

    private async Task OpenAsyncCommon(Chachar chachar, CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetAttributeAsync(
            IDs.CHACHAR_VIEW_DIALOG_DELETE_TOOLTIP,
            Attributes.DATA_BS_ORIGINAL_TITLE,
            string.Empty,
            cancellationToken
        );

        var databaseIntegrityErrorMessages = new List<string>();
        DeleteTitle = string.Empty;
        DisableDeleteCss = string.Empty;
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {chachar.TheCharacter}";
        Chachar = chachar;

        if (chachar.IsRadical)
        {
            var chacharsWithThisAsRadical = Index.Chachars.Where(chachar =>
                chachar.RadicalCharacter == Chachar.TheCharacter
                && chachar.RadicalPinyin == Chachar.Pinyin
                && chachar.RadicalTone == Chachar.Tone
            );

            if (chacharsWithThisAsRadical.Any())
            {
                databaseIntegrityErrorMessages.Add(Localizer[Resource.CharacterIsRadicalForOthers]);
                DisableDeleteCss = $"{CssClasses.DISABLED} {CssClasses.OPACITY_25}";
            }
        }

        var alternativesOfThis = Index.Alternatives.Where(alternative =>
            alternative.OriginalCharacter == Chachar!.TheCharacter
            && alternative.OriginalPinyin == Chachar.Pinyin
            && alternative.OriginalTone == Chachar.Tone
        );

        if (alternativesOfThis.Any())
        {
            databaseIntegrityErrorMessages.Add(Localizer[Resource.AlternativesExistForCharacter]);
            DisableDeleteCss = $"{CssClasses.DISABLED} {CssClasses.OPACITY_25}";
        }

        if (databaseIntegrityErrorMessages.Count > 0)
        {
            DeleteTitle = GetErrorMessageFormatted(databaseIntegrityErrorMessages);
            await InvokeAsync(StateHasChanged);
        }
    }

    private string GetErrorMessageFormatted(IEnumerable<string> databaseIntegrityErrorMessages)
    {
        var errorMessageBuilder = new StringBuilder(Localizer[Resource.CannotBeDeleted]);

        if (databaseIntegrityErrorMessages.Count() > 1)
        {
            foreach (var errorMessage in databaseIntegrityErrorMessages)
            {
                _ = errorMessageBuilder.Append(Html.BR).Append(Html.BULLET).Append(Html.NBSP).Append(errorMessage);
            }
        }
        else
        {
            _ = errorMessageBuilder.Append(' ').Append(databaseIntegrityErrorMessages.First());
        }

        return errorMessageBuilder.ToString();
    }

    private async Task SubmitDeleteAsync(CancellationToken cancellationToken)
    {
        await Index.ProcessDialog.SetProcessingAsync(this, cancellationToken);

        var isSuccess = await ModalCommons.SubmitAsync(
            this,
            Chachar!,
            Index,
            EntityClient.PostDeleteEntityAsync,
            HttpMethod.Post,
            ApiNames.CHARACTERS,
            Logger,
            Resource.CharacterDeleted,
            cancellationToken,
            Chachar!.TheCharacter!,
            Chachar.RealPinyin!
        );

        if (isSuccess)
        {
            _ = Index.Chachars.Remove(Chachar);
            await Index.StateHasChangedAsync();
        }
    }
}
