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
            modal.SetClasses(CssClasses.D_BLOCK);
            modalPage.SetBackdropClasses(CssClasses.D_BLOCK);
            await modal.StateHasChangedAsync();
            await modalPage.StateHasChangedAsync();
            await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
            modal.AddClasses(CssClasses.SHOW);
            modalPage.AddBackdropClasses(CssClasses.SHOW);
            await modal.StateHasChangedAsync();
            await modalPage.StateHasChangedAsync();
        }
        else
        {
            await LowerAllZIndexesAsync(modal.ModalLowerLevel!, NumberConstants.INDEX_BACKDROP_Z, cancellationToken);
            modal.SetClasses(CssClasses.D_BLOCK);
            await modal.StateHasChangedAsync();
            await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
            modal.AddClasses(CssClasses.SHOW);
            await modal.StateHasChangedAsync();
        }
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

        if (modal.Page is not null)
        {
            await CloseModalFirstLevelAsync(modal, cancellationToken);
        }
        else
        {
            await Task.WhenAll(
                _jSInteropDOM.SetTitleAsync(modal.ModalLowerLevel!.HtmlTitle, cancellationToken),
                CloseModalHigherLevelAsync(modal, cancellationToken)
            );
        }
    }

    public async Task CloseAllAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        ThrowOnInvalidModal(modal);

        if (modal.Page is not null)
        {
            await CloseModalFirstLevelAsync(modal, cancellationToken);
        }
        else
        {
            await Task.WhenAll(
                CloseModalHigherLevelAsync(modal, cancellationToken),
                CloseAllAsyncCommon(modal.ModalLowerLevel!, cancellationToken)
            );
        }
    }

    public async Task<bool> PostAsync<T>(
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

            await index.ProcessDialog.SetSuccessAsync(
                modal,
                string.Format(
                    CultureInfo.InvariantCulture,
                    _localizer[successMessageResource],
                    successMessageParams
                ),
                cancellationToken
            );

            _ = indexAlterCollection(entity);
            await index.StateHasChangedAsync();
            return true;
        }
        else
        {
            LogCommons.LogHttpMethodFailedError(logger, HttpMethod.Post);

            await index.ProcessDialog.SetErrorAsync(
                modal,
                _localizer[Resource.ProcessingError],
                cancellationToken
            );

            return false;
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

    private async Task CloseModalFirstLevelAsync(IModal modalFirstLevel, CancellationToken cancellationToken)
    {
        await _jSInteropDOM.SetTitleAsync(modalFirstLevel.Page!.HtmlTitle, cancellationToken);
        modalFirstLevel.SetClasses(CssClasses.D_BLOCK);
        modalFirstLevel.Page.SetBackdropClasses(CssClasses.D_BLOCK);
        await modalFirstLevel.StateHasChangedAsync();
        await modalFirstLevel.Page.StateHasChangedAsync();
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modalFirstLevel.SetClasses(CssClasses.D_NONE);
        modalFirstLevel.Page.SetBackdropClasses(CssClasses.D_NONE);
        await modalFirstLevel.StateHasChangedAsync();
        await modalFirstLevel.Page.StateHasChangedAsync();
    }

    private async Task CloseModalHigherLevelAsync(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        modal.SetClasses(CssClasses.D_BLOCK);
        await modal.StateHasChangedAsync();
        await IncreaseAllZIndexesAsync(modal.ModalLowerLevel!, NumberConstants.INDEX_BACKDROP_Z, cancellationToken);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modal.SetClasses(CssClasses.D_NONE);
        await modal.StateHasChangedAsync();
    }
}
