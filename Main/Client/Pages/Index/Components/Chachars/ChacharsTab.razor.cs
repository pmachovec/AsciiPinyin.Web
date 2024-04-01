using AsciiPinyin.Web.Client.Constants;
using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Chachars;

public class ChacharsTabBase : ComponentBase, IEntityTab
{
    public Chachar[]? Chachars { get; private set; }
    public bool IsVisible { get; private set; }
    public string Title { get; private set; } = StringConstants.CHARACTERS;

    public bool AreEntitiesInitialized => Chachars != null;

    protected ChacharViewDialog ChacharViewDialog = default!;

    [Inject]
    private IEntityLoader EntityLoader { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    protected override void OnInitialized()
        => Title = $"{Localizer[Resource.AsciiPinyin]} - {Localizer[Resource.Characters]}";

    public async void InitializeEntites()
    {
        Chachars = await EntityLoader.LoadEntitiesAsync<Chachar>("characters");
        JSInteropDOM.HideElement(IDs.CHACHARS_TAB_LOADING);
        StateHasChanged();
    }

    public void Hide()
    {
        IsVisible = false;
        JSInteropDOM.HideElement(IDs.CHACHARS_TAB_ROOT);
    }

    public void Show()
    {
        IsVisible = true;
        JSInteropDOM.ShowElement(IDs.CHACHARS_TAB_ROOT);
    }

    protected async void SelectChachar(Chachar chachar) => await ChacharViewDialog.SetChachar(chachar);
}
