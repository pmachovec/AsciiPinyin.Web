namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IEntityForm : IModalFirstLevel
{
    string HtmlTitle { get; }

    byte? Strokes { get; set; }

    string? TheCharacter { get; set; }
}
