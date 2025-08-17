using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using Bunit;

namespace AsciiPinyin.Web.Client.Test.Tools;

internal sealed class JSInteropSetter(BunitJSInterop _jsInterop)
{
    public void SetUpSetTitles(params string[] titles)
    {
        foreach (var title in titles)
        {
            _ = _jsInterop.SetupVoid(DOMFunctions.SET_TITLE, title).SetVoidResult();
        }
    }

    public void SetUpSetZIndex(string id)
    {
        _ = _jsInterop.SetupVoid(DOMFunctions.SET_Z_INDEX, id, 1).SetVoidResult();
        _ = _jsInterop.SetupVoid(DOMFunctions.SET_Z_INDEX, id, 3).SetVoidResult();
    }
}
