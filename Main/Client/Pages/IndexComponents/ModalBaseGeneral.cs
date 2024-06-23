namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public abstract class ModalBaseGeneral : ModalBase
{
    public abstract Task OpenAsync(CancellationToken cancellationToken);
}
