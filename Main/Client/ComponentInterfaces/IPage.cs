namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IPage
{
    string HtmlTitle { get; }

    void AddBackdropClasses(params string[] classes);

    void SetBackdropClasses(params string[] classes);

    Task StateHasChangedAsync();
}
