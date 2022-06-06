using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Components.Chachars;

public class ChacharCardsBase : ComponentBase
{
#pragma warning disable CS8618
    [Parameter]
    public Chachar[] Chachars { protected get; set; }

    [Parameter]
    public Action<Chachar> SelectChachar { protected get; set; }
#pragma warning restore CS8618

    protected int NumberOfLines { get; set; } = 10;
}
