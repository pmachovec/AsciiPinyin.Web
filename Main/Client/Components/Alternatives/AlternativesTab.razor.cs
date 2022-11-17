using AsciiPinyin.Web.Client.Shared.Constants;
using AsciiPinyin.Web.Client.Shared.EntityLoader;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Alternatives;

public class AlternativesTabBase : ComponentBase, IEntityTab
{
    protected const string ALTERNATIVES_TAB_DIV = "alternativesTabDiv";

    public Alternative[]? Alternatives { get; private set; }
    public bool IsVisible { get; private set; } = false;
    public string Title { get; private set; } = StringConstants.ALTERNATIVES;

    public bool AreEntitiesInitialized
    {
        get
        {
            return Alternatives != null;
        }
    }

    protected AlternativeViewDialog alternativeViewDialog = default!;

    [Inject]
    private IEntityLoader EntityLoader { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private ILokal Lokal { get; set; } = default!;

    protected override void OnInitialized()
    {
        Title = $"{Lokal.AsciiPinyin} - {Lokal.Alternatives}";
    }

    public async void InitializeEntites()
    {
        Alternatives = await EntityLoader.LoadEntitiesAsync<Alternative>("alternatives");
        StateHasChanged();
    }

    public void Hide()
    {
        IsVisible = false;
        JSInteropDOM.HideElement(ALTERNATIVES_TAB_DIV);
    }

    public void Show()
    {
        IsVisible = true;
        JSInteropDOM.ShowElement(ALTERNATIVES_TAB_DIV);
    }

    protected async void SelectAlternative(Alternative alternative)
    {
        await alternativeViewDialog.SetAlternative(alternative);
    }
}
