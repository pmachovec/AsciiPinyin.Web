using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace AsciiPinyin.Web.Server.Pages;

public class ErrorBase : ComponentBase
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    protected string? RequestId { get; set; }

    protected bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    protected override void OnInitialized() => RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
