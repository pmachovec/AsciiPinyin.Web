namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModalSecondLevel : IModal
{
    IModalFirstLevel ModalFirstLevel { get; set; }
}
