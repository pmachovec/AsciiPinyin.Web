using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class NewEntityCardBase : ComponentBase
{
    [Parameter]
    public Action ShowEntityForm { get; set; } = default!;
}
