using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Asciipinyin.Web.Server.Test.Commons;

internal static class ControllerTestCommons
{
    public static void PostFieldWrongTest(
        ObjectResult? result,
        object? value,
        string expectedErrorMessage,
        string fieldJsonPropertyName
    )
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));
        Assert.That(result!.Value, Is.InstanceOf<FieldErrorsContainer>());

        var fieldErrorsContainer = result.Value! as FieldErrorsContainer;
        var error = fieldErrorsContainer!.Errors[fieldJsonPropertyName];

        Assert.That(error, Is.Not.Null);
        Assert.That(error!.ErrorValue, Is.EqualTo(value));
        Assert.That(error!.ErrorMessage, Is.EqualTo(expectedErrorMessage));
        Assert.That(error!.FieldJsonPropertyName, Is.EqualTo(fieldJsonPropertyName));
    }
}
