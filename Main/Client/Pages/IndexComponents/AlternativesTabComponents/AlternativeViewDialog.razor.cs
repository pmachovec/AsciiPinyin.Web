using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ComponentBase, IModal
{
    protected Alternative? Alternative { get; set; }

    public string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    public string HtmlTitle { get; private set; } = string.Empty;

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

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
        ModalLowerLevel = null;
        Page = page;
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {alternative.TheCharacter}";
        Alternative = alternative;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
        StateHasChanged();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
        Alternative = null;
        StateHasChanged();
    }

    protected async Task InitiateDeleteAsync(CancellationToken cancellationToken)
    {
        await Index.SubmitDialog.SetWarningAsync(
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
    }

    private async Task SubmitDeleteAsync(CancellationToken cancellationToken) =>
        await ModalCommons.PostAsync(
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
