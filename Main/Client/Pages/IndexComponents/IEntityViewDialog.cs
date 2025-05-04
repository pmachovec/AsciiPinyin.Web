using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityViewDialog<T> : IModal, IComponent where T : IEntity
{
    Task OpenAsync(IPage page, T entity, CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);
}
