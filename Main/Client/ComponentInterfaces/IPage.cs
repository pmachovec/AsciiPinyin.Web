namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IPage
{
    string HtmlTitle { get; }

    IBackdrop Backdrop { get; }

    Task StateHasChangedAsync();
}
