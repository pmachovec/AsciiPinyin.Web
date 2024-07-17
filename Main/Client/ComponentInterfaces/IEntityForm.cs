namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IEntityForm : IModalFirstLevel
{
    byte? Strokes { get; set; }

    string? TheCharacter { get; set; }
}
