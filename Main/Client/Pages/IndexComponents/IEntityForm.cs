using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityForm<T> : IEntityModal where T : IEntity
{
    Task OpenAsync(IPage page, CancellationToken cancellationToken);

    Task OpenAsync(T entity, IModal modalLowerLevel, CancellationToken cancellationToken);
}
