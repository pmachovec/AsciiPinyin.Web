namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IEntityForm : IModalGeneral
{
    byte? Strokes { get; set; }

    string? TheCharacter { get; set; }
}
