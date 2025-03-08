using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Commons;

public interface IEntityFormCommons
{
    Task PreventMultipleCharactersAsync(
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken
    );

    Task PreventStrokesInvalidAsync(
        string inputId,
        ChangeEventArgs changeEventArgs,
        short? originalValue,
        CancellationToken cancellationToken
    );

    Task<short?> GetCorrectNumberInputValueAsync(
        string inputId,
        object? changeEventArgsValue,
        short? originalValue,
        CancellationToken cancellationToken
    );

    Task ClearWrongInputAsync(
        string inputId,
        string errorDivId,
        CancellationToken cancellationToken
    );

    Task<bool> CheckInputsAsync(
        CancellationToken cancellationToken,
        params (string inputId, string errorDivId, Func<string?> getErrorText)[] inputs
    );
}
