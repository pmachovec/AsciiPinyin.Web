using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.EntityLoader;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Chachars;

public class ChacharsTabBase : ComponentBase, IEntityTab
{
    protected const string CHACHARS_TAB_DIV = "chacharsTabDiv";

    public Chachar[]? Chachars { get; private set; }
    public bool IsVisible { get; private set; } = false;
    public string Title { get; private set; } = StringConstants.CHARACTERS;

    public bool AreEntitiesInitialized
    {
        get
        {
            return Chachars != null;
        }
    }

#pragma warning disable CS8618
    protected ChacharViewDialog chacharViewDialog;

    [Inject]
    private IEntityLoader EntityLoader { get; set; }

    [Inject]
    private ILokal Lokal { get; set; }

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; }

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; }
#pragma warning restore CS8618

    protected override void OnInitialized()
    {
        Title = $"{Lokal.AsciiPinyin} - {Lokal.Characters}";
    }

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

    protected async void SelectChachar(Chachar chachar)
    {
        await chacharViewDialog.SetChachar(chachar);
    }
}
