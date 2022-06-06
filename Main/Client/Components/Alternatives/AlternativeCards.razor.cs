using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Alternatives;

public class AlternativeCardsBase : ComponentBase
{
#pragma warning disable CS8618
    [Parameter]
    public Alternative[] Alternatives { protected get; set; }

    [Parameter]
    public Action<Alternative> SelectAlternative { protected get; set; }
#pragma warning restore CS8618sa
    protected int NumberOfLines { get; set; } = 10;
}
