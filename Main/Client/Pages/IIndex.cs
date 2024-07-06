using AsciiPinyin.Web.Shared.Models;

namespace AsciiPinyin.Web.Client.Pages;

public interface IIndex
{
    IEnumerable<Alternative> Alternatives { get; }

    IEnumerable<Chachar> Chachars { get; }
}
