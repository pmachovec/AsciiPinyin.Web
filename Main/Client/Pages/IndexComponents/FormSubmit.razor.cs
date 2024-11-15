using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class FormSubmitBase : ComponentBase, IEntityFormModal
{
    public string RootId { get; } = IDs.FORM_SUBMIT;

    public IEntityForm EntityForm { get; private set; } = default!;

    protected string HeaderText { get; private set; } = string.Empty;

    protected string BodyText { get; private set; } = string.Empty;

    protected string ButtonText { get; private set; } = string.Empty;

    protected Func<CancellationToken, Task> CloseAsync { get; private set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IModalCommons ModalCommons { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    public async Task SetProcessingAsync(
        IEntityForm entityForm,
        Task<HttpStatusCode> formSubmitTask,
        string messageOnSuccess,
        string messageOnError,
        CancellationToken cancellationToken
    )
    {
        await JSInteropDOM.SetTitleAsync($"{Localizer[Resource.Processing]}...", cancellationToken);
        EntityForm = entityForm;

        await Task.WhenAll(
            JSInteropDOM.Block2NoneAsync(IDs.FORM_SUBMIT_HEADER, cancellationToken),
            JSInteropDOM.Block2NoneAsync(IDs.FORM_SUBMIT_BODY_TEXT, cancellationToken),
            JSInteropDOM.Block2NoneAsync(IDs.FORM_SUBMIT_FOOTER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_LOADING, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );

        StateHasChanged();
        var formSubmitResult = await formSubmitTask;

        if (formSubmitResult == HttpStatusCode.OK)
        {
            await SetSuccessAsync(messageOnSuccess, cancellationToken);
        }
        else
        {
            await SetErrorAsync(entityForm, messageOnError, cancellationToken);
        }
    }

    private async Task SetSuccessAsync(string message, CancellationToken cancellationToken)
    {
        await JSInteropDOM.SetTitleAsync(Localizer[Resource.Success], cancellationToken);
        HeaderText = $"{Localizer[Resource.Success]}!";
        BodyText = message;
        ButtonText = Localizer[Resource.OK];
        CloseAsync = CloseSuccessAsync;
        StateHasChanged();

        await Task.WhenAll(
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_HEADER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_BODY_TEXT, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_FOOTER, cancellationToken),
            JSInteropDOM.Block2NoneAsync(IDs.FORM_SUBMIT_LOADING, cancellationToken),
            JSInteropDOM.RemoveClassAsync(IDs.FORM_SUBMIT_HEADER, CssClasses.BG_DANGER, cancellationToken),
            JSInteropDOM.AddClassAsync(IDs.FORM_SUBMIT_HEADER, CssClasses.BG_PRIMARY, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );
    }

    public async Task SetErrorAsync(
        IEntityForm entityForm,
        string message,
        CancellationToken cancellationToken
    )
    {
        await JSInteropDOM.SetTitleAsync(Localizer[Resource.Error], cancellationToken);
        EntityForm = entityForm;
        HeaderText = $"{Localizer[Resource.Error]}!";
        BodyText = message;
        ButtonText = Localizer[Resource.Back];
        CloseAsync = CloseErrorAsync;
        StateHasChanged();

        await Task.WhenAll(
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_HEADER, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_BODY_TEXT, cancellationToken),
            JSInteropDOM.None2BlockAsync(IDs.FORM_SUBMIT_FOOTER, cancellationToken),
            JSInteropDOM.Block2NoneAsync(IDs.FORM_SUBMIT_LOADING, cancellationToken),
            JSInteropDOM.RemoveClassAsync(IDs.FORM_SUBMIT_HEADER, CssClasses.BG_PRIMARY, cancellationToken),
            JSInteropDOM.AddClassAsync(IDs.FORM_SUBMIT_HEADER, CssClasses.BG_DANGER, cancellationToken),
            ModalCommons.OpenAsyncCommon(this, cancellationToken)
        );
    }

    protected async Task CloseSuccessAsync(CancellationToken cancellationToken)
        => await ModalCommons.CloseAllAsyncCommon(this, cancellationToken);

    protected async Task CloseErrorAsync(CancellationToken cancellationToken)
        => await ModalCommons.CloseAsyncCommon(this, cancellationToken);
}
