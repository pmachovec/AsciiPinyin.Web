using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public class EntityFormCommons(IJSInteropDOM _jSInteropDOM) : IEntityFormCommons
{
    public async Task PreventMultipleCharactersAsync(
        EntityFormBase entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken)
    {
        if (changeEventArgs.Value is string theCharacter)
        {
            if (theCharacter.Length <= 1 || TextUtils.GetStringRealLength(theCharacter) <= 1)
            {
                entityForm.TheCharacter = theCharacter;
            }
            else
            {
                var theCharacterStart = TextUtils.GetStringFirstCharacterAsString(theCharacter);
                entityForm.TheCharacter = theCharacterStart;
                await _jSInteropDOM.SetValueAsync(inputId, theCharacterStart, cancellationToken);
            }
        }
    }

    public async Task PreventStrokesInvalidAsync(
        EntityFormBase entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken)
    {
        entityForm.Strokes = await GetCorrectNumberInputValueAsync(
            inputId,
            changeEventArgs.Value,
            entityForm.Strokes,
            cancellationToken);
        await _jSInteropDOM.SetValueAsync(inputId, entityForm.Strokes.ToString()!, cancellationToken);
    }

    public async Task<byte?> GetCorrectNumberInputValueAsync(
        string inputId,
        object? changeEventArgsValue,
        byte? originalValue,
        CancellationToken cancellationToken)
    {
        // When the user types dot in an environment, where the dot is not decimal delimiter, the ChangeEventArgs value is empty string.
        // This is the only way how to distinguish typing the dot and having really empty number input in such situation.
        // If the dot is typed, the result is false, but with really empty input, it's true.
        var isInputValid = await _jSInteropDOM.IsValidInputAsync(inputId, cancellationToken);

        if (changeEventArgsValue is string emptyInputNumberAsStringEmpty
            && emptyInputNumberAsStringEmpty.Length == 0
            && isInputValid)
        {
            // At this point, the input is really empty.
            return null;
        }
        else
        {
            return isInputValid
                && changeEventArgsValue is string inputNumberAsString
                && byte.TryParse(inputNumberAsString.AsSpan(0, Math.Max(1, inputNumberAsString.Length)), out var inputNumber)
                ? inputNumber
                : originalValue;
        }
    }

    public async Task ClearWrongInputAsync(string inputId, string errorDivId, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
            _jSInteropDOM.RemoveTextAsync(errorDivId, cancellationToken));
    }

    public async Task<bool> AreAllInputsValidAsync(
        CancellationToken cancellationToken,
        params (string inputId, string errorDivId, Func<string?> getErrorText)[] inputs)
    {
        var separateCheckSuccesses = await Task.WhenAll(
            inputs.Select(input => CheckInputAsync(input.inputId, input.errorDivId, input.getErrorText, cancellationToken)));

        return separateCheckSuccesses.All(success => success);
    }

    private async Task<bool> CheckInputAsync(
        string inputId,
        string errorDivId,
        Func<string?> getErrorText,
        CancellationToken cancellationToken)
    {
        if (getErrorText() is { } errorText)
        {
            await Task.WhenAll(
                _jSInteropDOM.AddClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
                _jSInteropDOM.SetTextAsync(errorDivId, errorText, cancellationToken));

            return false;
        }

        return true;
    }
}
