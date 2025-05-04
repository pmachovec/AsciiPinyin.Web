using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ComponentBase, IEntityViewDialog<Alternative>
{
    protected Alternative? Alternative { get; set; }

    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string DisableDeleteCss { get; private set; } = string.Empty;

    protected string DeleteTitle { get; private set; } = string.Empty;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

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
        IPage page,
        Alternative alternative,
        CancellationToken cancellationToken
    )
    {
        await Index.ProcessDialog.SetProcessingAsync(this, cancellationToken);

        await JSInteropDOM.SetAttributeAsync(
            IDs.ALTERNATIVE_VIEW_DIALOG_DELETE_TOOLTIP,
            Attributes.DATA_BS_ORIGINAL_TITLE,
            string.Empty,
            cancellationToken
        );

        DeleteTitle = string.Empty;
        DisableDeleteCss = string.Empty;
        ModalLowerLevel = null;
        Page = page;
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {alternative.TheCharacter}";
        Alternative = alternative;

        var chacharsWithThis = Index.Chachars.Where(chachar =>
            chachar.RadicalAlternativeCharacter == Alternative!.TheCharacter
            && chachar.RadicalCharacter == Alternative.OriginalCharacter
            && chachar.RadicalPinyin == Alternative.OriginalPinyin
            && chachar.RadicalTone == Alternative.OriginalTone
        );

        if (chacharsWithThis.Any())
        {
            DisableDeleteCss = $"{CssClasses.DISABLED} {CssClasses.OPACITY_25}";
            DeleteTitle = $"{Localizer[Resource.CannotBeDeleted]} {Localizer[Resource.AlternativeUsedByCharactersInDb]}";
            await InvokeAsync(StateHasChanged);
        }

        await ModalCommons.OpenAsyncCommon(this, cancellationToken);
        await Index.ProcessDialog.CloseAsync(cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        Alternative = null;
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
    }

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected async Task InitiateDeleteAsync(CancellationToken cancellationToken) =>
        await Index.ProcessDialog.SetWarningAsync(
            this,
            string.Format(
                CultureInfo.InvariantCulture,
                Localizer[Resource.AlternativeWillBeDeleted],
                Alternative!.TheCharacter!,
                Alternative.OriginalCharacter!,
                Alternative.OriginalRealPinyin!
            ),
            SubmitDeleteAsync,
            cancellationToken
        );

    private async Task SubmitDeleteAsync(CancellationToken cancellationToken)
    {
        await Index.ProcessDialog.SetProcessingAsync(this, cancellationToken);

        _ = await ModalCommons.PostAsync(
            this,
            Alternative!,
            Index,
            EntityClient.PostDeleteEntityAsync,
            ApiNames.ALTERNATIVES,
            Logger,
            Index.Alternatives.Remove,
            Resource.AlternativeDeleted,
            cancellationToken,
            Alternative!.TheCharacter!,
            Alternative.OriginalCharacter!,
            Alternative.OriginalRealPinyin!
        );
    }
}
