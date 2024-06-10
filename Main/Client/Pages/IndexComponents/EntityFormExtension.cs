using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public static class EntityFormExtension
{
    internal static async Task PreventMultipleCharactersAsyncExtension(
        this EntityFormBase entityForm,
        IJSInteropDOM jSInteropDOM,
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
                await jSInteropDOM.SetValueAsync(inputId, theCharacterStart, cancellationToken);
            }
        }
    }

    internal static async Task PreventStrokesInvalidAsyncExtension(
        this EntityFormBase entityForm,
        IJSInteropDOM jSInteropDOM,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken)
    {
        entityForm.Strokes = await entityForm.GetCorrectNumberInputValueAsyncExtension(
            jSInteropDOM,
            IDs.CHACHAR_FORM_STROKES_INPUT,
            changeEventArgs.Value,
            entityForm.Strokes,
            cancellationToken);
        await jSInteropDOM.SetValueAsync(IDs.CHACHAR_FORM_STROKES_INPUT, entityForm.Strokes.ToString()!, cancellationToken);
    }

    internal static async Task<byte?> GetCorrectNumberInputValueAsyncExtension(
        this EntityFormBase _,
        IJSInteropDOM jSInteropDOM,
        string inputId,
        object? changeEventArgsValue,
        byte? originalValue,
        CancellationToken cancellationToken)
    {
        // When the user types dot in an environment, where the dot is not decimal delimiter, the ChangeEventArgs value is empty string.
        // This is the only way how to distinguish typing the dot and having really empty number input in such situation.
        // If the dot is typed, the result is false, but with really empty input, it's true.
        var isInputValid = await jSInteropDOM.IsValidInputAsync(inputId, cancellationToken);

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
}
