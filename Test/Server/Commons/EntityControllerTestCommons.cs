using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Test.Constants;
using AsciiPinyin.Web.Shared.DTO;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;

namespace AsciiPinyin.Web.Server.Test.Commons;

internal static class EntityControllerTestCommons
{
    public static void NoUserAgentHeaderTest<T>(ActionResult<T>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That((result.Result as BadRequestObjectResult)!.Value, Is.EqualTo(Errors.USER_AGENT_MISSING));
    }

    public static void InternalServerErrorTest<T>(ActionResult<T>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<StatusCodeResult>());

        var statusCodeResult = result.Result as StatusCodeResult;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    public static void GetAllEntitiesOkTest<T>(
        ActionResult<IEnumerable<T>>? result,
        params T[] expectedEntities
    ) where T : IEntity
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<OkObjectResult>());

        var value = (result.Result as OkObjectResult)!.Value;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!, Is.InstanceOf<IEnumerable<T>>());

        var entities = (value as IEnumerable<T>)!;
        Assert.That(entities.Count, Is.EqualTo(expectedEntities.Length));

        foreach (var expectedEntity in expectedEntities)
        {
            Assert.That(entities.Contains(expectedEntity), Is.True);
        }
    }

    public static void PostFieldWrongTest(
        ActionResult<IErrorsContainer>? result,
        string fieldName,
        object? fieldValue,
        string expectedErrorMessage
    ) => PostFieldsWrongTest(result, expectedErrorMessage, (fieldName, fieldValue));

    public static void PostFieldsWrongTest(
        ActionResult<IErrorsContainer>? result,
        string expectedErrorMessage,
        params (string, object?)[] fieldData
    )
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<BadRequestObjectResult>());

        var badRequestObjectResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.Not.Null);
        Assert.That(badRequestObjectResult.Value, Is.InstanceOf<EntityFieldsErrorsContainer>());

        var fieldsErrorsContainer = badRequestObjectResult.Value as EntityFieldsErrorsContainer;
        Assert.That(fieldsErrorsContainer!.Errors.Count, Is.EqualTo(1));

        var fieldsErrors = fieldsErrorsContainer!.Errors.First();

        foreach ((var fieldName, var fieldValue) in fieldData)
        {
            var error = fieldsErrors!.FieldErrors.FirstOrDefault(e => e.FieldName == fieldName);

            Assert.That(error, Is.Not.Null);
            Assert.That(error!.FieldValue, Is.EqualTo(fieldValue));
            Assert.That(error!.ErrorMessage, Is.EqualTo(expectedErrorMessage));
        }
    }

    public static void PostDatabaseIntegrityErrorTest(
        ActionResult<IErrorsContainer>? result,
        string expectedEntityType,
        IEntity expectedEntity,
        string expectedErrorMessage,
        params ConflictEntity[] expectedConflictEntities
    )
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<BadRequestObjectResult>());

        var badRequestObjectResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.Not.Null);
        Assert.That(badRequestObjectResult.Value, Is.InstanceOf<DatabaseIntegrityErrorsContainer>());

        var databaseIntegrityErrorsContainer = badRequestObjectResult.Value as DatabaseIntegrityErrorsContainer;
        Assert.That(databaseIntegrityErrorsContainer!.Errors.Count, Is.EqualTo(1));

        var error = databaseIntegrityErrorsContainer.Errors.First();
        Assert.That(error.EntityType, Is.EqualTo(expectedEntityType));
        Assert.That(error.Entity, Is.EqualTo(expectedEntity));
        Assert.That(error.ErrorMessage, Is.EqualTo(expectedErrorMessage));
        Assert.That(error.ConflictEntities.Count, Is.EqualTo(expectedConflictEntities.Length));

        foreach (var expectedConflictEntity in expectedConflictEntities)
        {
            var conflictEntity = error.ConflictEntities.FirstOrDefault(c => c.EntityType == expectedConflictEntity.EntityType);

            Assert.That(conflictEntity, Is.Not.Null);
            Assert.That(conflictEntity!.Entity, Is.EqualTo(expectedConflictEntity.Entity));
        }
    }

    public static void PostOkTest(ActionResult<IErrorsContainer>? result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<OkResult>());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    public static Mock<DbSet<Chachar>> GetChacharDbSetMock(params Chachar[] data)
    {
        var dataQueryable = data.AsQueryable();
        var dbSetMock = new Mock<DbSet<Chachar>>();

        _ = dbSetMock
            .Setup(m => m.Find(It.IsAny<object[]>()))
            .Returns((object[] parameters) =>
                {
                    if (
                        parameters.Length == 3
                        && parameters[0] is string theCharacter
                        && parameters[1] is string pinyin
                        && parameters[2] is byte tone
                    )
                    {
                        return data.FirstOrDefault(d =>
                            d.TheCharacter == theCharacter
                            && d.Pinyin == pinyin
                            && d.Tone == tone
                        );
                    }

                    return null;
                }
            );

        _ = dbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
        _ = dbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
        _ = dbSetMock.As<IQueryable<Chachar>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
        _ = dbSetMock.As<IQueryable<Chachar>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator);

        return dbSetMock;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    public static Mock<DbSet<Alternative>> GetAlternativeDbSetMock(params Alternative[] data)
    {
        var dataQueryable = data.AsQueryable();
        var dbSetMock = new Mock<DbSet<Alternative>>();

        _ = dbSetMock
            .Setup(m => m.Find(It.IsAny<object[]>()))
            .Returns((object[] parameters) =>
                {
                    if (
                        parameters.Length == 4
                        && parameters[0] is string theCharacter
                        && parameters[1] is string originalCharacter
                        && parameters[2] is string originalPinyin
                        && parameters[3] is byte originalTone
                    )
                    {
                        return data.FirstOrDefault(d =>
                            d.TheCharacter == theCharacter
                            && d.OriginalCharacter == originalCharacter
                            && d.OriginalPinyin == originalPinyin
                            && d.OriginalTone == originalTone
                        );
                    }

                    return null;
                }
            );

        _ = dbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
        _ = dbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
        _ = dbSetMock.As<IQueryable<Alternative>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
        _ = dbSetMock.As<IQueryable<Alternative>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator);

        return dbSetMock;
    }

    public static void MockDatabaseFacadeTransaction(Mock<AsciiPinyinContext> asciiPinyinContextMock)
    {
        var databaseFacadeMock = new Mock<DatabaseFacade>(asciiPinyinContextMock.Object);
        var dbContextTransactionMock = new Mock<IDbContextTransaction>();
        _ = databaseFacadeMock.Setup(m => m.BeginTransaction()).Returns(dbContextTransactionMock.Object);
        _ = asciiPinyinContextMock.Setup(context => context.Database).Returns(databaseFacadeMock.Object);
    }

    public static string GetEntityUnknownErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' does not identify an existing {entityType}";

    public static string GetEntityExistsErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies an already existing {entityType}";

    public static string GetNoRadicalErrorMessage(params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' identifies a chachar, which is not radical";
}
