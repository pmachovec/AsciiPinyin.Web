using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityModal<T> : IModal, IComponent where T : IEntity
{
    Task OpenAsync(T entity, IModal modalLowerLevel, CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);
}
