using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Components;
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
    public async Task OpenAsyncCommon(IModal modal, CancellationToken cancellationToken)
    {
        ThrowOnInvalidModal(modal);

        if (modal.ModalLowerLevel is { } modalLowerLevel)
        {
            await OpenHigherLevelAsync(modal, modalLowerLevel, cancellationToken);
        }
        else
        {
            await OpenFirstLevelAsync(modal, modal.Page!, cancellationToken);
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

        if (modal.ModalLowerLevel is { } modalLowerLevel)
        {
            await Task.WhenAll(
                _jSInteropDOM.SetTitleAsync(modalLowerLevel.HtmlTitle, cancellationToken),
                CloseHigherLevelAsync(modal, modalLowerLevel, cancellationToken)
            );
        }
        else
        {
            await CloseWithoutBackdropAsync(modal, cancellationToken);
        }
    }

    public async Task CloseAllAsyncCommon(
        IModal modal,
        CancellationToken cancellationToken
    )
    {
        var (modals, backdrops, title) = GetModalsBackdropsTitle(modal);
        await _jSInteropDOM.SetTitleAsync(title, cancellationToken);

        foreach (var anotherModal in modals)
        {
            anotherModal.SetClasses(CssClasses.D_BLOCK);
        }

        foreach (var backdrop in backdrops)
        {
            backdrop.SetClasses(CssClasses.D_BLOCK);
        }

        await Task.WhenAll(modals.Select(modal => modal.StateHasChangedAsync()));
        await Task.WhenAll(backdrops.Select(backdrop => backdrop.StateHasChangedAsync()));
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);

        foreach (var anotherModal in modals)
        {
            anotherModal.SetClasses(CssClasses.D_NONE);
        }

        foreach (var backdrop in backdrops)
        {
            backdrop.SetClasses(CssClasses.D_NONE);
        }

        await Task.WhenAll(modals.Select(modal => modal.StateHasChangedAsync()));
        await Task.WhenAll(backdrops.Select(backdrop => backdrop.StateHasChangedAsync()));
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

    private static async Task OpenFirstLevelAsync(IModal modal, IPage page, CancellationToken cancellationToken)
    {
        modal.SetClasses(CssClasses.D_BLOCK);
        modal.Backdrop = page.Backdrop;
        modal.Backdrop.SetClasses(CssClasses.D_BLOCK);
        modal.Backdrop.ZIndex = NumberConstants.BACKDROP_INITIAL_INDEX;
        modal.ZIndex = modal.Backdrop.ZIndex + 1;

        await Task.WhenAll(
            modal.StateHasChangedAsync(),
            modal.Backdrop.StateHasChangedAsync()
        );

        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        modal.AddClasses(CssClasses.SHOW);
        modal.Backdrop.AddClasses(CssClasses.SHOW);

        await Task.WhenAll(
            modal.StateHasChangedAsync(),
            modal.Backdrop.StateHasChangedAsync()
        );
    }

    private static async Task OpenHigherLevelAsync(IModal modal, IModal modalLowerLevel, CancellationToken cancellationToken)
    {
        modal.ZIndex = modalLowerLevel.ZIndex + 1;
        modal.SetClasses(CssClasses.D_BLOCK);
        List<Task> awaitables = [modal.StateHasChangedAsync()];

        if (modalLowerLevel.Backdrop is not null)
        {
            modal.Backdrop = modalLowerLevel.Backdrop;
            modal.Backdrop.ZIndex += 1;
            modalLowerLevel.ZIndex -= 1;
            modalLowerLevel.Backdrop = null;
            awaitables.Add(modalLowerLevel.StateHasChangedAsync());
            awaitables.Add(modal.Backdrop.StateHasChangedAsync());
        }

        await Task.WhenAll(awaitables);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        modal.AddClasses(CssClasses.SHOW);
        await modal.StateHasChangedAsync();
    }

    private static async Task CloseWithoutBackdropAsync(IModal modal, CancellationToken cancellationToken)
    {
        modal.SetClasses(CssClasses.D_BLOCK);
        await modal.StateHasChangedAsync();
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modal.SetClasses(CssClasses.D_NONE);
        await modal.StateHasChangedAsync();
    }

    private static async Task CloseHigherLevelAsync(IModal modal, IModal modalLowerLevel, CancellationToken cancellationToken)
    {
        List<Task> awaitables = [];

        if (modal.Backdrop is { } backdrop)
        {
            modalLowerLevel.Backdrop = backdrop;
            modalLowerLevel.Backdrop.ZIndex -= 1;
            modalLowerLevel.ZIndex += 1;
            awaitables.Add(modalLowerLevel.StateHasChangedAsync());
            awaitables.Add(modalLowerLevel.Backdrop.StateHasChangedAsync());
        }

        modal.SetClasses(CssClasses.D_BLOCK);
        await Task.WhenAll(awaitables);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modal.SetClasses(CssClasses.D_NONE);
        await modal.StateHasChangedAsync();
    }

    private static (List<IModal>, List<IBackdrop>, string) GetModalsBackdropsTitle(IModal modal)
    {
        ThrowOnInvalidModal(modal);

        List<IModal> modals;
        List<IBackdrop> backdrops;
        string title;

        if (modal.ModalLowerLevel is { } modalLowerLevel)
        {
            (modals, backdrops, title) = GetModalsBackdropsTitle(modalLowerLevel);
        }
        else
        {
            modals = [];
            backdrops = [];
            title = modal.Page!.HtmlTitle;
        }

        modals.Add(modal);

        if (modal.Backdrop is { } backdrop)
        {
            backdrops.Add(backdrop);
        }

        return (modals, backdrops, title);
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
}
