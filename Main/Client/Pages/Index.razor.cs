using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using NLog;
using System.Text.Json;

namespace AsciiPinyin.Web.Client.Pages;

public class IndexBase : ComponentBase, IIndex
{
    private static IEntityTab? _selectedTab;

    protected AlternativesTab AlternativesTab { get; set; } = default!;

    protected ChacharsTab ChacharsTab { get; set; } = default!;

    protected string BackdropClasses { get; private set; } = CssClasses.D_NONE;

    public string HtmlTitle { get; private set; } = string.Empty;

    public IProcessDialog ProcessDialog { get; set; } = default!;

    public IEntityTab SelectedTab => _selectedTab!;

    public ISet<Alternative> Alternatives { get; private set; } = new HashSet<Alternative>();

    public ISet<Chachar> Chachars { get; private set; } = new HashSet<Chachar>();

    [Inject]
    private IJSInteropConsole JSInteropConsole { get; set; } = default!;

    [Inject]
    private ILogger<Index> Logger { get; set; } = default!;

    [Inject]
    private IJSInteropDOM JSInteropDOM { get; set; } = default!;

    public void AddBackdropClasses(params string[] classes) => BackdropClasses += $" {string.Join(' ', classes)}";

    public void SetBackdropClasses(params string[] classes) => BackdropClasses = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);

    protected override async Task OnInitializedAsync()
    {
        // Must be here, NLog.Configuration is null before starting the app with "RunAsync()".
        InjectJSInteropConsoleToTargets();

        await Task.WhenAll(
            JSInteropDOM.SetTitleAsync(StringConstants.ASCII_PINYIN, CancellationToken.None),
            JSInteropDOM.HideElementAsync(IDs.LOADING_SPLASH_NAVBAR, CancellationToken.None)
        );
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var alternativesPreload = await JSInteropDOM.GetTextAsync(IDs.APP_ALTERNATIVES_PRELOAD, CancellationToken.None);
            var chacharsPreload = await JSInteropDOM.GetTextAsync(IDs.APP_CHACHARS_PRELOAD, CancellationToken.None);

            if (DeserializePreload<Alternative>(alternativesPreload, ApiNames.ALTERNATIVES) is ISet<Alternative> alternatives)
            {
                Alternatives = alternatives;
            }

            if (DeserializePreload<Chachar>(chacharsPreload, ApiNames.CHACHARS) is ISet<Chachar> chachars)
            {
                Chachars = chachars;
            }

            await Task.WhenAll(
                SelectTabAsync(ChacharsTab, CancellationToken.None),
                JSInteropDOM.HideElementAsync(IDs.LOADING_SPLASH_WHEEL, CancellationToken.None)
            );
        }
    }

    protected async Task SelectTabAsync(
        IEntityTab tab,
        CancellationToken cancellationToken
    )
    {
        HtmlTitle = tab.HtmlTitle;

        if (_selectedTab is { } selectedTab)
        {
            await selectedTab.HideAsync(cancellationToken);
            selectedTab.Classes = string.Empty;
        }

        tab.Classes = CssClasses.ACTIVE;
        await tab.ShowAsync(cancellationToken);
        _selectedTab = tab;
        StateHasChanged();
    }

    private void InjectJSInteropConsoleToTargets()
    {
        var jsInteropConsoleTargets = LogManager.Configuration.AllTargets.OfType<JSInteropConsoleTarget>();

        foreach (var jsInteropConsoleTarget in jsInteropConsoleTargets)
        {
            jsInteropConsoleTarget.JSInteropConsole = JSInteropConsole;
        }
    }

    private ISet<T>? DeserializePreload<T>(
        string preload,
        string entitiesName
    ) where T : IEntity
    {
        if (preload is null)
        {
            Logger.LogWarning("Preloaded {entitiesName} string is null.", entitiesName);
            return null;
        }

        if (preload.Length == 0)
        {
            Logger.LogWarning("Preloaded {entitiesName} string is empty.", entitiesName);
            return null;
        }

        ISet<T>? entities = null;

        try
        {
            entities = JsonSerializer.Deserialize<ISet<T>>(preload);
        }
        catch (Exception ex)
        {
            LogCommons.LogExceptionError(Logger, ex);
        }

        if (entities is null)
        {
            Logger.LogError("Failed to deserialize preloaded {entitiesName}.", entitiesName);
            Logger.LogError("Preloaded {entitiesName} string: {preload}", entitiesName, preload);
        }

        return entities;
    }
}
