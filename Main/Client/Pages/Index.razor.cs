using AsciiPinyin.Web.Client.EntityClient;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase, IIndex
{
    private IEntityTab? _selectedTab;

    protected AlternativesTab AlternativesTab { get; set; } = default!;

    protected ChacharsTab ChacharsTab { get; set; } = default!;

    public IEnumerable<Alternative> Alternatives { get; private set; } = [];

    public IEnumerable<Chachar> Chachars { get; private set; } = [];

    [Inject]
    private IEntityClient EntityClient { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            JSInteropDOM.SetTitleAsync(StringConstants.ASCII_PINYIN, CancellationToken.None),
            JSInteropDOM.HideElementAsync(IDs.LOADING_SPLASH, CancellationToken.None)
        );
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Alternatives = await EntityClient.LoadEntitiesAsync<Alternative>(ApiNames.ALTERNATIVES, CancellationToken.None);
            Chachars = await EntityClient.LoadEntitiesAsync<Chachar>(ApiNames.CHARACTERS, CancellationToken.None);

            await Task.WhenAll(
                SelectTabAsync(ChacharsTab, CancellationToken.None),
                JSInteropDOM.HideElementAsync(IDs.INDEX_ENTITIES_TABS_LOADING, CancellationToken.None));
        }
    }

    protected async Task SelectTabAsync(
        IEntityTab tab,
        CancellationToken cancellationToken)
    {
        if (_selectedTab is { } selectedTab)
        {
            await Task.WhenAll(
                selectedTab.HideAsync(cancellationToken),
                JSInteropDOM.RemoveClassAsync(selectedTab.ButtonId, CssClasses.ACTIVE, cancellationToken));
        }

        await Task.WhenAll(
            JSInteropDOM.AddClassAsync(tab.ButtonId, CssClasses.ACTIVE, cancellationToken),
            tab.ShowAsync(cancellationToken));

        _selectedTab = tab;
        StateHasChanged();
    }

    protected static string GetActiveIfVisible(IEntityTab? tab)
        => tab != null && tab.IsVisible ? "active" : string.Empty;
}
