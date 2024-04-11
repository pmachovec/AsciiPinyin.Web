using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Server.Pages;

public class AppBase : ComponentBase
{
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;
}
