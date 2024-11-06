using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Asciipinyin.Web.Server.Test.Commons;

internal static class EntityControllerTestCommons
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

    public static Mock<DbSet<T>> GetDbSetMock<T>(params T[] data) where T : class
    {
        var dataQueryable = data.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();
        _ = dbSetMock.Setup(m => m.Find(It.IsAny<object[]>())).Returns(data.FirstOrDefault);
        _ = dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
        _ = dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
        _ = dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
        _ = dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator);

        return dbSetMock;
    }
}
