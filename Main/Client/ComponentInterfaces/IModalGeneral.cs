namespace AsciiPinyin.Web.Client.ComponentInterfaces;

public interface IModalGeneral : IModal
{
    Task OpenAsync(CancellationToken cancellationToken);
}
