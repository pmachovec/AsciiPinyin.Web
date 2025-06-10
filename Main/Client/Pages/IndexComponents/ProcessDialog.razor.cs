using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class ProcessDialogBase : ComponentBase, IProcessDialog
{
    protected MarkupString BodyText { get; private set; } = new(string.Empty);

    protected string BodyTextClasses { get; private set; } = string.Empty;

    protected string ButtonBackClasses { get; private set; } = string.Empty;

    protected string ButtonProceedClasses { get; private set; } = string.Empty;

    protected string ButtonProceedText { get; private set; } = string.Empty;

    protected string Classes { get; private set; } = CssClasses.D_NONE;

    protected string FooterClasses { get; private set; } = CssClasses.D_NONE;

    protected string HeaderClasses { get; private set; } = string.Empty;

    protected string LoadingClasses { get; private set; } = string.Empty;

    protected string HeaderText { get; private set; } = string.Empty;

    protected Func<CancellationToken, Task> ProceedAsync { get; private set; } = default!;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IBackdrop? Backdrop { get; set; }

    public IModal? ModalLowerLevel { get; private set; }

    public IPage? Page { get; private set; }

    public string RootId { get; } = IDs.PROCESS_DIALOG;

    public int ZIndex { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task SetProcessingAsync(IPage page, CancellationToken cancellationToken)
    {
        ModalLowerLevel = null;
        Page = page;
        await SetProcessingAsync(cancellationToken);
    }

    public async Task SetProcessingAsync(IModal modalLowerLevel, CancellationToken cancellationToken)
    {
        ModalLowerLevel = modalLowerLevel;
        Page = null;
        await SetProcessingAsync(cancellationToken);
    }

    public async Task SetSuccessAsync(
        IModal modalLowerLevel,
        string message,
        CancellationToken cancellationToken
    )
    {
        ModalLowerLevel = modalLowerLevel;
        Page = null;
        HtmlTitle = Localizer[Resource.Success];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);

        HeaderText = $"{Localizer[Resource.Success]}!";
        BodyText = new(message);
        ButtonProceedText = Localizer[Resource.OK];
        ProceedAsync = CloseAllAsync;

        LoadingClasses = CssClasses.D_NONE;
        HeaderClasses = $"{CssClasses.D_FLEX} {CssClasses.BG_PRIMARY}";
        BodyTextClasses = CssClasses.D_BLOCK;
        ButtonBackClasses = CssClasses.D_NONE;
        ButtonProceedClasses = CssClasses.D_BLOCK;
        FooterClasses = CssClasses.D_FLEX;

        await ModalCommons.OpenAsyncCommon(this, cancellationToken);
        await StateHasChangedAsync();
    }

    public async Task SetErrorAsync(
        IModal modalLowerLevel,
        string message,
        CancellationToken cancellationToken
    )
    {
        ModalLowerLevel = modalLowerLevel;
        Page = null;
        HtmlTitle = Localizer[Resource.Error];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);

        HeaderText = $"{Localizer[Resource.Error]}!";
        BodyText = new(message);

        LoadingClasses = CssClasses.D_NONE;
        HeaderClasses = $"{CssClasses.D_FLEX} {CssClasses.BG_DANGER}";
        BodyTextClasses = CssClasses.D_BLOCK;
        ButtonBackClasses = CssClasses.D_BLOCK;
        ButtonProceedClasses = CssClasses.D_NONE;
        FooterClasses = CssClasses.D_FLEX;

        await ModalCommons.OpenAsyncCommon(this, cancellationToken);
        await StateHasChangedAsync();
    }

    public async Task SetWarningAsync(
        IModal modalLowerLevel,
        string message,
        Func<CancellationToken, Task> methodOnProceedAsync,
        CancellationToken cancellationToken
    )
    {
        ModalLowerLevel = modalLowerLevel;
        Page = null;
        HtmlTitle = Localizer[Resource.Warning];
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);

        HeaderText = $"{Localizer[Resource.Warning]}!";
        BodyText = new(message);
        ButtonProceedText = Localizer[Resource.Proceed];
        ProceedAsync = methodOnProceedAsync;

        LoadingClasses = CssClasses.D_NONE;
        HeaderClasses = $"{CssClasses.D_FLEX} {CssClasses.BG_WARNING}";
        BodyTextClasses = CssClasses.D_BLOCK;
        ButtonBackClasses = CssClasses.D_BLOCK;
        ButtonProceedClasses = CssClasses.D_BLOCK;
        FooterClasses = CssClasses.D_FLEX;

        await ModalCommons.OpenAsyncCommon(this, cancellationToken);
        await StateHasChangedAsync();
    }

    public async Task CloseAsync(CancellationToken cancellationToken) =>
        await ModalCommons.CloseAsyncCommon(this, cancellationToken);

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected async Task CloseAllAsync(CancellationToken cancellationToken)
        => await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);

    private async Task SetProcessingAsync(CancellationToken cancellationToken)
    {
        HtmlTitle = $"{Localizer[Resource.Processing]}...";
        await JSInteropDOM.SetTitleAsync(HtmlTitle, cancellationToken);

        LoadingClasses = CssClasses.D_BLOCK;
        HeaderClasses = CssClasses.D_NONE;
        BodyTextClasses = CssClasses.D_NONE;
        FooterClasses = CssClasses.D_NONE;

        await ModalCommons.OpenAsyncCommon(this, cancellationToken);
        await StateHasChangedAsync();
    }
}
