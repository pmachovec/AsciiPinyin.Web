namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModal
{
    string RootId { get; }

    string HtmlTitleOnClose { get; set; }
}
