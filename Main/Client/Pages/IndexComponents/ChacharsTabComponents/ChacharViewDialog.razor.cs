using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents.ChacharsTabComponents;

public class ChacharViewDialogBase : ComponentBase, IModalEntitySpecific<Chachar>
{
    protected Chachar? Chachar { get; set; }

    public string RootId { get; } = IDs.CHACHAR_VIEW_DIALOG_ROOT;

    public event EventHandler EventOnClose = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task OpenAsync(Chachar entity, CancellationToken cancellationToken)
    {
        await ModalCommons.OpenAsyncCommon(
            this,
            $"{StringConstants.ASCII_PINYIN} - {entity.TheCharacter}",
            cancellationToken);

        Chachar = entity;
        StateHasChanged();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await ModalCommons.CloseAsyncCommon(
            this,
            EventOnClose,
            cancellationToken);

        Chachar = null;
        StateHasChanged();
    }
}
