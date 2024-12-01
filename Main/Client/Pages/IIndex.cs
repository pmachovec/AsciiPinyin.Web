using AsciiPinyin.Web.Client.ComponentInterfaces;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.ComponentInterfaces;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages;

public interface IIndex : IPage
{
    SubmitDialog SubmitDialog { get; }

    IEntityTab SelectedTab { get; }

    ISet<Alternative> Alternatives { get; }

    ISet<Chachar> Chachars { get; }

    void StateHasChangedPublic();
}
