using Microsoft.AspNetCore.Components;

namespace AsciiPinyin.Web.Client.Pages.IndexComponents;

public interface IEntityFormCommons
{
    Task PreventMultipleCharactersAsyncCommon(
        EntityFormBase entityForm,
        string inputId,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken);

    Task PreventStrokesInvalidAsyncCommon(
        EntityFormBase entityForm,
        ChangeEventArgs changeEventArgs,
        CancellationToken cancellationToken);

    Task<byte?> GetCorrectNumberInputValueAsyncCommon(
        string inputId,
        object? changeEventArgsValue,
        byte? originalValue,
        CancellationToken cancellationToken);
}
