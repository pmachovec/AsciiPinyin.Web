using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages;

public interface IIndex
{
    SaveSuccess SaveSuccess { get; }

    SaveFailed SaveFailed { get; }

    IEnumerable<Alternative> Alternatives { get; }

    IEnumerable<Chachar> Chachars { get; }
}
