using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Pages;
using AsciiPinyin.Web.Shared.Models;
using System.Net;

namespace AsciiPinyin.Web.Client.Commons;

public interface IModalCommons
{
    Task OpenFirstLevelAsyncCommon(IModal modal, string htmlTitle, CancellationToken cancellationToken);

    Task OpenFirstLevelAsyncCommon(IModal modal, CancellationToken cancellationToken);

    Task OpenHigherLevelAsyncCommon(IModal modal, string htmlTitle, CancellationToken cancellationToken);

    Task OpenHigherLevelAsyncCommon(IModal modal, CancellationToken cancellationToken);

    Task CloseHigherLevelAsyncCommon(IModal modal, CancellationToken cancellationToken);

    Task CloseWithoutBackdropAsyncCommon(IModal modal, CancellationToken cancellationToken);

    Task CloseAllAsyncCommon(IModal modal, CancellationToken cancellationToken);

    Task PostAsync<T>(
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
    ) where T : IEntity;
}
