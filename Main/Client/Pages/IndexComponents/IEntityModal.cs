using AsciiPinyin.Web.Client.ComponentInterfaces;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityModal : IModal, IComponent
{
    Task CloseAsync(CancellationToken cancellationToken);
}
