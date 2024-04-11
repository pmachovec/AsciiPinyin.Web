namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public abstract class ModalWithBackdropBaseGeneral : ModalWithBackdropBase
{
    public abstract Task OpenAsync(CancellationToken cancellationToken);
}
