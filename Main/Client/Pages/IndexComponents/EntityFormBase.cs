namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public abstract class EntityFormBase : ModalWithBackdropBaseGeneral
{
    public abstract byte? Strokes { get; set; }

    public abstract string? TheCharacter { get; set; }
}
