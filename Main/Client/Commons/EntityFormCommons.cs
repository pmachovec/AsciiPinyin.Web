using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Pages.IndexComponents;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Resources;
using AsciiPinyin.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Commons;

public sealed class EntityFormCommons(
    IJSInteropDOM _jSInteropDOM,
    IStringLocalizer<Resource> _localizer
) : IEntityFormCommons
{
    public async Task PreventMultipleCharactersAsync(
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken
    )
    {
        if (
            changeEventArgs.Value is string theCharacter
            && theCharacter.Length > 1
            && TextUtils.GetStringRealLength(theCharacter) > 1
        )
        {
            var theCharacterStart = TextUtils.GetStringFirstCharacterAsString(theCharacter);
            await _jSInteropDOM.SetValueAsync(inputId, theCharacterStart, cancellationToken);
        }
    }

    public async Task PreventStrokesInvalidAsync(
        string inputId,
        ChangeEventArgs changeEventArgs,
        short? originalValue,
        CancellationToken cancellationToken
    )
    {
        var correctStrokes = await GetCorrectNumberInputValueAsync(
            inputId,
            changeEventArgs.Value,
            originalValue,
            cancellationToken
        );

        await _jSInteropDOM.SetValueAsync(
            inputId,
            correctStrokes?.ToString(CultureInfo.InvariantCulture)!,
            cancellationToken
        );
    }

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

    public async Task ClearWrongInputAsync(
        string inputId,
        string errorDivId,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _jSInteropDOM.RemoveClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
            _jSInteropDOM.RemoveTextAsync(errorDivId, cancellationToken)
        );
    }

    public async Task<bool> CheckInputsAsync(
        CancellationToken cancellationToken,
        params (string inputId, string errorDivId, Func<string?> getErrorText)[] inputs
    )
    {
        var separateCheckSuccesses = await Task.WhenAll(
            inputs.Select(input => CheckInputAsync(input.inputId, input.errorDivId, input.getErrorText, cancellationToken))
        );

        return separateCheckSuccesses.All(success => success);
    }

    public string? GetNullInputErrorText(object? theInput) =>
        theInput is null ? (string)_localizer[Resource.CompulsoryValue] : null;

    private async Task<bool> CheckInputAsync(
        string inputId,
        string errorDivId,
        Func<string?> getErrorText,
        CancellationToken cancellationToken
    )
    {
        if (getErrorText() is { } errorText)
        {
            await Task.WhenAll(
                _jSInteropDOM.AddClassAsync(inputId, CssClasses.BORDER_DANGER, cancellationToken),
                _jSInteropDOM.SetTextAsync(errorDivId, errorText, cancellationToken)
            );

            return false;
        }

        return true;
    }
}
