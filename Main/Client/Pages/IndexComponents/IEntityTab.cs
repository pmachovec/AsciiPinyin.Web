namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityTab
{
    bool AreEntitiesInitialized { get; }
    bool IsVisible { get; }
    string Title { get; }
    void InitializeEntites();
    void Hide();
    void Show();
}
