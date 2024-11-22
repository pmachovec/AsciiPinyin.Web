using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Test.Constants;
using AsciiPinyin.Web.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;

namespace Asciipinyin.Web.Server.Test.Commons;

internal static class EntityControllerTestCommons
{
    public static void NoUserAgentHeaderTest(ActionResult<FieldErrorsContainer>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That((result.Result as BadRequestObjectResult)!.Value, Is.EqualTo(Errors.USER_AGENT_MISSING));
    }

    public static void InternalServerErrorTest(ActionResult<FieldErrorsContainer>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<StatusCodeResult>());

        var statusCodeResult = result.Result as StatusCodeResult;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    public static void PostFieldWrongTest(
        ActionResult<FieldErrorsContainer>? result,
        string expectedErrorMessage,
        object? value,
        string fieldJsonPropertyName
    ) => PostFieldsWrongTest(result, expectedErrorMessage, (value, fieldJsonPropertyName));

    public static void PostFieldsWrongTest(
        ActionResult<FieldErrorsContainer>? result,
        string expectedErrorMessage,
        params (object? value, string fieldJsonPropertyName)[] fieldData
    )
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<BadRequestObjectResult>());

        var badRequestObjectResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.Not.Null);
        Assert.That(badRequestObjectResult.Value, Is.InstanceOf<FieldErrorsContainer>());

        var fieldErrorsContainer = badRequestObjectResult.Value as FieldErrorsContainer;

        foreach ((var value, var fieldJsonPropertyName) in fieldData)
        {
            var error = fieldErrorsContainer!.Errors[fieldJsonPropertyName];

            Assert.That(error, Is.Not.Null);
            Assert.That(error!.ErrorValue, Is.EqualTo(value));
            Assert.That(error!.ErrorMessage, Is.EqualTo(expectedErrorMessage));
            Assert.That(error!.FieldJsonPropertyName, Is.EqualTo(fieldJsonPropertyName));
        }
    }

    public static void PostOkTest(ActionResult<FieldErrorsContainer>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<OkResult>());
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

    public static void MockDatabaseFacadeTransaction(Mock<AsciiPinyinContext> asciiPinyinContextMock)
    {
        var databaseFacadeMock = new Mock<DatabaseFacade>(asciiPinyinContextMock.Object);
        var dbContextTransactionMock = new Mock<IDbContextTransaction>();
        _ = databaseFacadeMock.Setup(m => m.BeginTransaction()).Returns(dbContextTransactionMock.Object);
        _ = asciiPinyinContextMock.Setup(context => context.Database).Returns(databaseFacadeMock.Object);
    }
}
