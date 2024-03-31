using AsciiPinyin.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Shared.Components;

public class LoadingBase : ComponentBase
{
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; } = default!;
}
