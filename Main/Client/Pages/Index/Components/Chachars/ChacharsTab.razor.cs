using AsciiPinyin.Web.Client.Constants;
using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Chachars;

public class ChacharsTabBase : ComponentBase, IEntityTab
{
    protected const string CHACHARS_TAB_DIV = "chacharsTabDiv";

    public Chachar[]? Chachars { get; private set; }
    public bool IsVisible { get; private set; }
    public string Title { get; private set; } = StringConstants.CHARACTERS;

    public bool AreEntitiesInitialized => Chachars != null;

    protected ChacharViewDialog ChacharViewDialog = default!;

    [Inject]
    private IEntityLoader EntityLoader { get; set; } = default!;

    [Inject]
    private ILokal Lokal { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    protected override void OnInitialized() => Title = $"{Lokal.AsciiPinyin} - {Lokal.Characters}";

    public async void InitializeEntites()
    {
        Chachars = await EntityLoader.LoadEntitiesAsync<Chachar>("characters");
        StateHasChanged();
    }

    public void Hide()
    {
        IsVisible = false;
        JSInteropDOM.HideElement(CHACHARS_TAB_DIV);
    }

    public void Show()
    {
        IsVisible = true;
        JSInteropDOM.ShowElement(CHACHARS_TAB_DIV);
    }

    protected async void SelectChachar(Chachar chachar) => await ChacharViewDialog.SetChachar(chachar);
}
