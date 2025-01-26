using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharViewDialogBase : ComponentBase, IModal
{
    protected Chachar? Chachar { get; set; }

    public string RootId { get; } = IDs.CHACHAR_VIEW_DIALOG_ROOT;

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
        Chachar chachar,
        CancellationToken cancellationToken
    )
    {
        ModalLowerLevel = null;
        Page = page;
        HtmlTitle = $"{StringConstants.ASCII_PINYIN} - {chachar.TheCharacter}";
        Chachar = chachar;
        await ModalCommons.OpenAsyncCommon(this, HtmlTitle, cancellationToken);
        StateHasChanged();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
        Chachar = null;
        StateHasChanged();
    }

    protected async Task InitiateDeleteAsync(CancellationToken cancellationToken)
    {
        await Index.SubmitDialog.SetWarningAsync(
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
    }

    private async Task SubmitDeleteAsync(CancellationToken cancellationToken) =>
        await ModalCommons.PostAsync(
            this,
            Chachar!,
            Index,
            EntityClient.PostDeleteEntityAsync,
            ApiNames.CHARACTERS,
            Logger,
            Index.Chachars.Remove,
            Resource.CharacterDeleted,
            cancellationToken,
            Chachar!.TheCharacter!,
            Chachar.RealPinyin!
        );
}
