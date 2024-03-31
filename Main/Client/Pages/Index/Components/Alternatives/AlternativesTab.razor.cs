using AsciiPinyin.Web.Client.Constants;
using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Alternatives;

public class AlternativesTabBase : ComponentBase, IEntityTab
{
    protected const string ALTERNATIVES_TAB_DIV = "alternativesTabDiv";

    public Alternative[]? Alternatives { get; private set; }
    public bool IsVisible { get; private set; }
    public string Title { get; private set; } = StringConstants.ALTERNATIVES;

    public bool AreEntitiesInitialized => Alternatives != null;

    protected AlternativeViewDialog AlternativeViewDialog = default!;

    [Inject]
    private IEntityLoader EntityLoader { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    private IStringLocalizer<Resource> Localizer { get; set; } = default!;

    [Inject]
    protected IJSInteropConsole JSInteropConsole { get; set; } = default!;

    protected override void OnInitialized()
        => Title = $"{Localizer[Resource.AsciiPinyin]} - {Localizer[Resource.Alternatives]}";

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

    protected async void SelectAlternative(Alternative alternative) => await AlternativeViewDialog.SetAlternative(alternative);
}
