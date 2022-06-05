using Microsoft.JSInterop;

namespace AsciiPinyin.Web.Client.Shared.JSInterop;

public class JSInteropDOM
{
    public static async void SetTitle(string title, IJSRuntime jsRuntime)
    {
        await jsRuntime.InvokeVoidAsync("setTitle", title);
    }
}
