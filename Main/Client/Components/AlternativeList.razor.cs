using AsciiPinyin.Web.Client.Shared;
using AsciiPinyin.Web.Client.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AsciiPinyin.Web.Client.Components;

public class AlternativeListBase : ComponentBase
{
    #pragma warning disable CS8618
    [Inject]
    protected IStringLocalizer<Resource> Localizer { get; set; }
    #pragma warning restore CS8618

    protected string GetLocalizedString(string theString)
    {
        return SafeLocalization.GetLocalizedString(Localizer, theString, "AlternativeListBase");
    }
}
