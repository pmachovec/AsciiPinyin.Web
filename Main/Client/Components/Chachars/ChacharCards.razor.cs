using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Chachars;

public class ChacharCardsBase : ComponentBase
{
    [Parameter]
    public Chachar[] Chachars { get; set; } = default!;

    [Parameter]
    public Action<Chachar> SelectChachar { get; set; } = default!;

    protected int NumberOfLines { get; set; } = 10;
}
