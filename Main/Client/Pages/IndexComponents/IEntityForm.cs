using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityForm<T> : IEntityModal<T> where T : IEntity
{
    Task OpenAsync(IPage page, CancellationToken cancellationToken);
}
