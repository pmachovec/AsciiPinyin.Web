using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components;

public class BackdropBase : ComponentBase, IBackdrop
{
    protected string Classes { get; private set; } = CssClasses.D_NONE;

    public int ZIndex { get; set; } = NumberConstants.BACKDROP_INITIAL_INDEX;

    [Parameter, EditorRequired]
    public required string RootId { get; init; }

    public void AddClasses(params string[] classes) => Classes += $" {string.Join(' ', classes)}";

    public void SetClasses(params string[] classes) => Classes = string.Join(' ', classes);

    public async Task StateHasChangedAsync() => await InvokeAsync(StateHasChanged);
}
