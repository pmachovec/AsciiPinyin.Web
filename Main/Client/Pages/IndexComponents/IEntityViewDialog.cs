using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityViewDialog<T> : IEntityModal where T : IEntity
{
    Task OpenAsync(T entity, IPage page, CancellationToken cancellationToken);
}
