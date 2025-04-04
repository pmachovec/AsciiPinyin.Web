using AsciiPinyin.Web.Server.Data;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace AsciiPinyin.Web.Server.Components;

public class AppBase : ComponentBase
{
    [Inject]
    private AsciiPinyinContext AsciiPinyinContext { get; set; } = default!;

    protected string AlternativesPreload = string.Empty;
    protected string ChacharsPreload = string.Empty;

    protected override void OnInitialized()
    {
        AlternativesPreload = JsonSerializer.Serialize(AsciiPinyinContext.Alternatives);
        ChacharsPreload = JsonSerializer.Serialize(AsciiPinyinContext.Chachars);
    }
}
