using AsciiPinyin.Web.Client.ComponentInterfaces;
using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Commons;

public interface IEntityFormCommons
{
    EventHandler GetModalToFrontEvent(IModal modal, string titleToSet);

    Task PreventMultipleCharactersAsync(
        IEntityForm entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken);

    Task PreventStrokesInvalidAsync(
        IEntityForm entityForm,
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

    string? GetNullInputErrorText(object? theInput);
}
