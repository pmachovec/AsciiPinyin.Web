using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModal : IComponent
{
    string HtmlTitle { get; }

    IBackdrop? Backdrop { get; set; }

    IModal? ModalLowerLevel { get; }

    IPage? Page { get; }

    string RootId { get; }

    int ZIndex { get; set; }

    void AddClasses(params string[] classes);

    void SetClasses(params string[] classes);

    Task StateHasChangedAsync();
}
