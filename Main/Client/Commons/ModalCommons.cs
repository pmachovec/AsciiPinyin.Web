using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Exceptions;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class ModalCommons(
    IJSInteropDOM _jSInteropDOM,
    IStringLocalizer<Resource> _localizer
) : IModalCommons
{
    public async Task OpenAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        ThrowOnInvalidModal(modal);

        if (modal.Page is { } modalPage)
        {
            await _jSInteropDOM.None2BlockAsync(modalPage.BackdropId, cancellationToken);
            await _jSInteropDOM.AddClassAsync(modalPage.BackdropId, CssClasses.SHOW, cancellationToken);
        }
        else
        {
            await LowerAllZIndexesAsync(modal.ModalLowerLevel!, ByteConstants.INDEX_BACKDROP_Z, cancellationToken);
        }

        await _jSInteropDOM.None2BlockAsync(modal.RootId, cancellationToken);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        await _jSInteropDOM.AddClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken);
    }

    public async Task OpenAsyncCommon(
        IModal modal,
        string htmlTitle,
        CancellationToken cancellationToken
    )
    {
        ThrowOnInvalidModal(modal);
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await OpenAsyncCommon(modal, cancellationToken);
    }

    public async Task CloseAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        ThrowOnInvalidModal(modal);

        if (modal.Page is { } modalPage)
        {
            await _jSInteropDOM.SetTitleAsync(modalPage.HtmlTitle, cancellationToken);
            await _jSInteropDOM.RemoveClassAsync(modalPage.BackdropId, CssClasses.SHOW, cancellationToken);
            await _jSInteropDOM.Display2NoneAsync(modalPage.BackdropId, cancellationToken);
        }
        else
        {
            await _jSInteropDOM.SetTitleAsync(modal.ModalLowerLevel!.HtmlTitle, cancellationToken);
            await IncreaseAllZIndexesAsync(modal.ModalLowerLevel!, ByteConstants.INDEX_BACKDROP_Z, cancellationToken);
        }

        await CloseAsyncCommonCommon(modal, cancellationToken);
    }

    public async Task CloseAllAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        ThrowOnInvalidModal(modal);

        if (modal.Page is { } modalPage)
        {
            await _jSInteropDOM.SetTitleAsync(modalPage.HtmlTitle, cancellationToken);
            await _jSInteropDOM.RemoveClassAsync(modalPage.BackdropId, CssClasses.SHOW, cancellationToken);
            await _jSInteropDOM.Display2NoneAsync(modalPage.BackdropId, cancellationToken);
            await CloseAsyncCommonCommon(modal, cancellationToken);
        }
        else
        {
            await Task.WhenAll(
                CloseAllAsyncCommon(modal.ModalLowerLevel!, cancellationToken),
                CloseAsyncCommon(modal, cancellationToken)
            );
        }
    }

    public async Task PostAsync<T>(
        IModal modal,
        T entity,
        IIndex index,
        Func<string, T, CancellationToken, Task<HttpStatusCode>> entityClientPostAsync,
        string entitiesApiName,
        ILogger<IModal> logger,
        Func<T, bool> indexAlterCollection,
        string successMessageResource,
        CancellationToken cancellationToken,
        params string[] successMessageParams
    ) where T : IEntity
    {
        var postResult = await entityClientPostAsync(entitiesApiName, entity, cancellationToken);

        if (postResult == HttpStatusCode.OK)
        {
            LogCommons.LogHttpMethodSuccessInfo(logger, HttpMethod.Post);

            await index.SubmitDialog.SetSuccessAsync(
                modal,
                string.Format(
                    CultureInfo.InvariantCulture,
                    _localizer[successMessageResource],
                    successMessageParams
                ),
                cancellationToken
            );

            _ = indexAlterCollection(entity);
            index.StateHasChangedPublic();
        }
        else
        {
            LogCommons.LogHttpMethodFailedError(logger, HttpMethod.Post);
            await index.SubmitDialog.SetErrorAsync(
                modal,
                _localizer[Resource.ProcessingError],
                cancellationToken
            );
        }
    }

    private static void ThrowOnInvalidModal(IModal modal)
    {
        if (modal.Page is not null && modal.ModalLowerLevel is not null)
        {
            throw new InvalidModalException(
                $"Modal '{modal.RootId}' has configured both the page and the lower level modal."
                + " It must have configured exactly one of them."
            );
        }

        if (modal.Page is null && modal.ModalLowerLevel is null)
        {
            throw new InvalidModalException(
                $"Modal '{modal.RootId}' has configured neither the page nor the lower level modal."
                + " It must have configured exactly one of them."
            );
        }
    }

    private async Task LowerAllZIndexesAsync(IModal modal, int higherLevelZIndex, CancellationToken cancellationToken)
    {
        if (modal.ModalLowerLevel is { } modalLowerLevel)
        {
            await LowerAllZIndexesAsync(modalLowerLevel, higherLevelZIndex - 1, cancellationToken);
        }

        await _jSInteropDOM.SetZIndexAsync(
            modal.RootId,
            higherLevelZIndex - 1,
            cancellationToken
        );
    }

    private async Task IncreaseAllZIndexesAsync(IModal modal, int lowerLevelZIndex, CancellationToken cancellationToken)
    {
        await _jSInteropDOM.SetZIndexAsync(
            modal.RootId,
            lowerLevelZIndex + 1,
            cancellationToken
        );

        if (modal.ModalLowerLevel is { } modalLowerLevel)
        {
            await IncreaseAllZIndexesAsync(modalLowerLevel, lowerLevelZIndex + 1, cancellationToken);
        }
    }

    private async Task CloseAsyncCommonCommon(IModal modal, CancellationToken cancellationToken)
    {
        await _jSInteropDOM.RemoveClassAsync(modal.RootId, CssClasses.SHOW, cancellationToken);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        await _jSInteropDOM.Display2NoneAsync(modal.RootId, cancellationToken);
    }
}
