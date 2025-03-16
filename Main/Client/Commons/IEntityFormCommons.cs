namespace AsciiPinyin.Web.Client.Commons;

public interface IEntityFormCommons
{
    Task<short?> GetCorrectNumberInputValueAsync(
        string inputId,
        object? changeEventArgsValue,
        short? originalValue,
        CancellationToken cancellationToken
    );
}
