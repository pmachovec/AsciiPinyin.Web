using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityFormCommons
{
    Task PreventMultipleCharactersAsync(
        EntityFormBase entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken);

    Task PreventStrokesInvalidAsync(
        EntityFormBase entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken);

    Task<byte?> GetCorrectNumberInputValueAsync(
        string inputId,
        object? changeEventArgsValue,
        byte? originalValue,
        CancellationToken cancellationToken);

    Task ClearWrongInputAsync(
        string inputId,
        string errorDivId,
        CancellationToken cancellationToken);

    Task<bool> CheckInputsAsync(
        CancellationToken cancellationToken,
        params (string inputId, string errorDivId, Func<string?> getErrorText)[] inputs);
}
