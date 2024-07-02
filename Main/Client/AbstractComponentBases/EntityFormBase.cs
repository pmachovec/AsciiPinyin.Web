namespace AsciiPinyin.Web.Client.AbstractComponentBases;

public abstract class EntityFormBase : ModalBaseGeneral
{
    public abstract byte? Strokes { get; set; }

    public abstract string? TheCharacter { get; set; }
}
