namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModal
{
    string HtmlTitle { get; }

    IModal? ModalLowerLevel { get; }

    IPage? Page { get; }

    string RootId { get; }

    void AddClasses(params string[] classes);

    void SetClasses(params string[] classes);

    Task StateHasChangedAsync();
}
