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
using System.Text;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class ModalCommons(
    IJSInteropDOM _jSInteropDOM,
    IStringLocalizer<Resource> _localizer
) : IModalCommons
{
    public async Task OpenFirstLevelAsyncCommon(IModal modal, string htmlTitle, CancellationToken cancellationToken)
    {
        ThrowOnInvalidFirstLevel(modal);
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await OpenFirstLevelAsyncCommon(modal, cancellationToken);
    }

    public async Task OpenFirstLevelAsyncCommon(IModal modal, CancellationToken cancellationToken)
    {
        ThrowOnInvalidFirstLevel(modal);
        modal.SetClasses(CssClasses.D_BLOCK);
        modal.Backdrop = modal.Page!.Backdrop;
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

    public async Task OpenHigherLevelAsyncCommon(IModal modal, string htmlTitle, CancellationToken cancellationToken)
    {
        ThrowOnInvalidHigherLevel(modal);
        await _jSInteropDOM.SetTitleAsync(htmlTitle, cancellationToken);
        await OpenHigherLevelAsyncCommon(modal, cancellationToken);
    }

    public async Task OpenHigherLevelAsyncCommon(IModal modal, CancellationToken cancellationToken)
    {
        ThrowOnInvalidHigherLevel(modal);
        modal.ZIndex = modal.ModalLowerLevel!.ZIndex + 1;
        modal.SetClasses(CssClasses.D_BLOCK);
        List<Task> awaitables = [modal.StateHasChangedAsync()];

        if (modal.ModalLowerLevel.Backdrop is not null)
        {
            modal.Backdrop = modal.ModalLowerLevel.Backdrop;
            modal.Backdrop.ZIndex += 1;
            modal.ModalLowerLevel.ZIndex -= 1;
            modal.ModalLowerLevel.Backdrop = null;
            awaitables.Add(modal.ModalLowerLevel.StateHasChangedAsync());
            awaitables.Add(modal.Backdrop.StateHasChangedAsync());
        }

        await Task.WhenAll(awaitables);
        await Task.Delay(IntConstants.MODAL_SHOW_DELAY, cancellationToken);
        modal.AddClasses(CssClasses.SHOW);
        await modal.StateHasChangedAsync();
    }

    public async Task CloseHigherLevelAsyncCommon(IModal modal, CancellationToken cancellationToken)
    {
        ThrowOnInvalidHigherLevel(modal);
        List<Task> awaitables = [_jSInteropDOM.SetTitleAsync(modal.ModalLowerLevel!.HtmlTitle, cancellationToken)];

        if (modal.Backdrop is { } backdrop)
        {
            modal.ModalLowerLevel.Backdrop = backdrop;
            modal.ModalLowerLevel.Backdrop.ZIndex -= 1;
            modal.ModalLowerLevel.ZIndex += 1;
            awaitables.Add(modal.ModalLowerLevel.StateHasChangedAsync());
            awaitables.Add(modal.ModalLowerLevel.Backdrop.StateHasChangedAsync());
            modal.Backdrop = null;
        }

        modal.SetClasses(CssClasses.D_BLOCK);
        await modal.StateHasChangedAsync();
        await Task.WhenAll(awaitables);
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modal.SetClasses(CssClasses.D_NONE);
        await modal.StateHasChangedAsync();
    }

    public async Task CloseWithoutBackdropAsyncCommon(IModal modal, CancellationToken cancellationToken)
    {
        ThrowOnInvalidModal(modal);
        modal.SetClasses(CssClasses.D_BLOCK);
        await modal.StateHasChangedAsync();
        await Task.Delay(IntConstants.MODAL_HIDE_DELAY, cancellationToken);
        modal.SetClasses(CssClasses.D_NONE);
        await modal.StateHasChangedAsync();
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

        await Task.WhenAll(
            modals.Select(modal =>
            {
                modal.Backdrop = null;
                return modal.StateHasChangedAsync();
            })
        );

        await Task.WhenAll(backdrops.Select(backdrop => backdrop.StateHasChangedAsync()));
    }

    public async Task<bool> SubmitAsync<T>(
        IModal modal,
        T entity,
        IIndex index,
        Func<string, T, CancellationToken, Task<HttpStatusCode>> entityClientSubmitAsync,
        HttpMethod httpMethod,
        string entitiesApiName,
        ILogger<IModal> logger,
        string successMessageResource,
        CancellationToken cancellationToken,
        params string[] successMessageParams
    ) where T : IEntity
    {
        var postResult = await entityClientSubmitAsync(entitiesApiName, entity, cancellationToken);

        if (postResult == HttpStatusCode.OK)
        {
            LogCommons.LogHttpMethodSuccessInfo(logger, httpMethod);

            await index.ProcessDialog.SetSuccessAsync(
                modal,
                string.Format(
                    CultureInfo.InvariantCulture,
                    _localizer[successMessageResource],
                    successMessageParams
                ),
                cancellationToken
            );

            return true;
        }
        else
        {
            LogCommons.LogHttpMethodFailedError(logger, httpMethod);

            await index.ProcessDialog.SetErrorAsync(
                modal,
                _localizer[Resource.ProcessingError],
                cancellationToken
            );

            return false;
        }
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

    private static void ThrowOnInvalidFirstLevel(IModal modal)
    {
        StringBuilder? errorMessageBuilder = null;

        if (modal.ModalLowerLevel is not null)
        {
            errorMessageBuilder = new($"Modal '{modal.RootId}' has configured lower level modal.");
        }

        if (modal.Page is null)
        {
            var errorMessage = $"Modal '{modal.RootId}' has not configured the page.";

            if (errorMessageBuilder is null)
            {
                errorMessageBuilder = new(errorMessage);
            }
            else
            {
                _ = errorMessageBuilder.Append(' ').Append(errorMessage);
            }
        }

        if (errorMessageBuilder is not null)
        {
            throw new InvalidModalException(errorMessageBuilder.ToString());
        }
    }

    private static void ThrowOnInvalidHigherLevel(IModal modal)
    {
        StringBuilder? errorMessageBuilder = null;

        if (modal.Page is not null)
        {
            errorMessageBuilder = new($"Modal '{modal.RootId}' has configured the page.");
        }

        if (modal.ModalLowerLevel is null)
        {
            var errorMessage = $"Modal '{modal.RootId}' has not configured the lower level modal.";

            if (errorMessageBuilder is null)
            {
                errorMessageBuilder = new(errorMessage);
            }
            else
            {
                _ = errorMessageBuilder.Append(' ').Append(errorMessage);
            }
        }

        if (errorMessageBuilder is not null)
        {
            throw new InvalidModalException(errorMessageBuilder.ToString());
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
}
