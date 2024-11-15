using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ComponentBase, IModalFirstLevel
{
    protected Alternative? Alternative { get; set; }

    public string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Parameter]
    public required IIndex Index { get; set; } = default!;

    public async Task OpenAsync(
        Alternative alternative,
        CancellationToken cancellationToken
    )
    {
        await ModalCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {alternative.TheCharacter}",
            cancellationToken
        );

        Alternative = alternative;
        StateHasChanged();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);
        Alternative = null;
        StateHasChanged();
    }
}
