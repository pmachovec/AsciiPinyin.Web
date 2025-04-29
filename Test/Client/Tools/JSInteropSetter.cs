using AsciiPinyin.Web.Client.Test.Constants.JSInterop;
using Bunit;

namespace Asciipinyin.Web.Client.Test.Tools;

internal sealed class JSInteropSetter(BunitJSInterop _jsInterop)
{
    public void SetUpDisable(params string[] ids)
    {
        foreach (var id in ids)
        {
            _ = _jsInterop.SetupVoid(DOMFunctions.DISABLE, id).SetVoidResult();
        }
    }

    public void SetUpEnable(params string[] ids)
    {
        foreach (var id in ids)
        {
            _ = _jsInterop.SetupVoid(DOMFunctions.ENABLE, id).SetVoidResult();
        }
    }

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
