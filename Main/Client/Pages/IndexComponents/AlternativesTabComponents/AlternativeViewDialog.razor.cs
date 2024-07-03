using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.AlternativesTabComponents;

public class AlternativeViewDialogBase : ComponentBase, IModalEntitySpecific<Alternative>
{
    protected Alternative? Alternative { get; set; }

    public string RootId { get; } = IDs.ALTERNATIVE_VIEW_DIALOG_ROOT;

    public event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task OpenAsync(Alternative entity, CancellationToken cancellationToken)
    {
        await ModalCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);

        Alternative = entity;
        StateHasChanged();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);

        Alternative = null;
        StateHasChanged();
    }
}
