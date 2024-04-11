namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityTab
{
    bool IsVisible { get; }

    Task HideAsync(CancellationToken cancellationToken);

    Task ShowAsync(CancellationToken cancellationToken);
}
