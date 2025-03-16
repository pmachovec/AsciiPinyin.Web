using AsciiPinyin.Web.Client.JSInterop;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class EntityFormCommons(IJSInteropDOM _jSInteropDOM) : IEntityFormCommons
{
    public async Task<short?> GetCorrectNumberInputValueAsync(
        string inputId,
        object? changeEventArgsValue,
        short? originalValue,
        CancellationToken cancellationToken
    )
    {
        // When the user types dot in an environment, where the dot is not decimal delimiter, the ChangeEventArgs value is empty string.
        // This is the only way how to distinguish typing the dot and having really empty number input in such situation.
        // If the dot is typed, the result is false, but with really empty input, it's true.
        var isInputValid = await _jSInteropDOM.IsValidInputAsync(inputId, cancellationToken);

        if (
            changeEventArgsValue is string emptyInputNumberAsStringEmpty
            && emptyInputNumberAsStringEmpty.Length == 0
            && isInputValid
        )
        {
            // At this point, the input is really empty.
            return null;
        }
        else
        {
            return (
                isInputValid
                && changeEventArgsValue is string inputNumberAsString
                && short.TryParse(inputNumberAsString.AsSpan(0, Math.Max(1, inputNumberAsString.Length)), out var inputNumber)
            )
            ? inputNumber : originalValue;
        }
    }
}
