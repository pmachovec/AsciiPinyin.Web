using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.Index.Components.Alternatives;

public class AlternativeCardsBase : ComponentBase
{
    [Parameter]
    public Alternative[] Alternatives { get; set; } = default!;

    [Parameter]
    public Action<Alternative> SelectAlternative { get; set; } = default!;

    protected int NumberOfLines { get; set; } = 10;

    protected static void ShowAlternativeForm()
    {
        //TODO
    }
}
