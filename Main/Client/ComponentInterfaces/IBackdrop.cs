namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IBackdrop
{
    int ZIndex { get; set; }

    void AddClasses(params string[] classes);

    void SetClasses(params string[] classes);

    Task StateHasChangedAsync();
}
