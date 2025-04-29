namespace AsciiPinyin.Web.Shared.ComponentInterfaces;

public interface IEntityTab
{
    string Classes { get; set; }

    string HtmlTitle { get; }

    bool IsVisible { get; }

    Task HideAsync(CancellationToken cancellationToken);

    Task ShowAsync(CancellationToken cancellationToken);
}
