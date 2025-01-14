using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class SubmitDialogBase : ComponentBase, ISubmitDialog
{
    protected string HeaderText { get; private set; } = string.Empty;

    protected MarkupString BodyText { get; private set; } = new(string.Empty);

    protected string ButtonProceedText { get; private set; } = string.Empty;

    protected Func<CancellationToken, Task> ProceedAsync { get; private set; } = default!;

    public string RootId { get; } = IDs.SUBMIT_DIALOG;

    public IPage? Page { get; private set; }

    public IModal? ModalLowerLevel { get; private set; }

    public string HtmlTitle { get; private set; } = string.Empty;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task SetProcessingAsync(IEntityForm entityForm, CancellationToken cancellationToken)
    {
        Page = null;
        ModalLowerLevel = entityForm;
        HtmlTitle = $"{Localizer[Resource.Processing]}...";
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);

        await Task.WhenAll(
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_BODY_TEXT, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_FOOTER, cancellationToken),
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_LOADING, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );

        StateHasChanged();
    }

    public async Task SetSuccessAsync(
        IModal modalLowerLevel,
        string message,
        CancellationToken cancellationToken
    )
    {
        Page = null;
        ModalLowerLevel = modalLowerLevel;
        HtmlTitle = Localizer[Resource.Success];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        HeaderText = $"{Localizer[Resource.Success]}!";
        BodyText = new(message);
        ButtonProceedText = Localizer[Resource.OK];
        ProceedAsync = CloseAllAsync;
        StateHasChanged();

        await Task.WhenAll(
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_FOOTER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BODY_TEXT, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BUTTON_PROCEED, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_BUTTON_BACK, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_LOADING, cancellationToken),
            JSInteropDOM.SetBgPrimaryAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );
    }

    public async Task SetErrorAsync(
        IModal modalLowerLevel,
        string message,
        CancellationToken cancellationToken
    )
    {
        Page = null;
        ModalLowerLevel = modalLowerLevel;
        HtmlTitle = Localizer[Resource.Error];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        HeaderText = $"{Localizer[Resource.Error]}!";
        BodyText = new(message);
        StateHasChanged();

        await Task.WhenAll(
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_FOOTER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BODY_TEXT, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BUTTON_BACK, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_BUTTON_PROCEED, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_LOADING, cancellationToken),
            JSInteropDOM.SetBgDangerAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );
    }

    public async Task SetWarningAsync(
        IModal modalLowerLevel,
        string message,
        Func<CancellationToken, Task> methodOnProceedAsync,
        CancellationToken cancellationToken
    )
    {
        Page = null;
        ModalLowerLevel = modalLowerLevel;
        HtmlTitle = Localizer[Resource.Warning];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);
        HeaderText = $"{Localizer[Resource.Warning]}!";
        BodyText = new(message);
        ButtonProceedText = Localizer[Resource.Proceed];
        ProceedAsync = methodOnProceedAsync;
        StateHasChanged();

        await Task.WhenAll(
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            JSInteropDOM.None2FlexAsync(IDs.SUBMIT_DIALOG_FOOTER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BODY_TEXT, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BUTTON_BACK, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.SUBMIT_DIALOG_BUTTON_PROCEED, cancellationToken),
            JSInteropDOM.Display2NoneAsync(IDs.SUBMIT_DIALOG_LOADING, cancellationToken),
            JSInteropDOM.SetBgWarningAsync(IDs.SUBMIT_DIALOG_HEADER, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );
    }

    protected async Task CloseAllAsync(CancellationToken cancellationToken)
        => await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);
}
