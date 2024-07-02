namespace AsciiPinyin.Web.Shared.ComponentInterfaces;

public interface IEntityTab
{
    string ButtonId { get; }

    bool IsVisible { get; }

    Task HideAsync(CancellationToken cancellationToken);

    Task ShowAsync(CancellationToken cancellationToken);
}
