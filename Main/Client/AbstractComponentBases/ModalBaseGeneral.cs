namespace AsciiPinyin.Web.Client.AbstractComponentBases;

public abstract class ModalBaseGeneral : ModalBase
{
    public abstract Task OpenAsync(CancellationToken cancellationToken);
}
