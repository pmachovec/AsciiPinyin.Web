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

    public static void PostDatabaseSingleIntegrityErrorTest(
        ActionResult<IErrorsContainer>? result,
        string expectedEntityType,
        IEntity expectedEntity,
        string expectedErrorMessage,
        params ConflictEntity[] expectedConflictEntities
    ) =>
        PostDatabaseIntegrityErrorsTest(
            result,
            (expectedEntityType, expectedEntity, expectedErrorMessage, expectedConflictEntities)
        );

    public static void PostDatabaseIntegrityErrorsTest(
        ActionResult<IErrorsContainer>? result,
        params (
            string ExpectedEntityType,
            IEntity ExpectedEntity,
            string ExpectedErrorMessage,
            ConflictEntity[] ExpectedConflictEntities
        )[] expectedDatabaseIntegrityErrorsData
    )
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.Not.Null);
        Assert.That(result.Result!, Is.InstanceOf<BadRequestObjectResult>());

        var badRequestObjectResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.Not.Null);
        Assert.That(badRequestObjectResult.Value, Is.InstanceOf<DatabaseIntegrityErrorsContainer>());

        var databaseIntegrityErrorsContainer = badRequestObjectResult.Value as DatabaseIntegrityErrorsContainer;
        Assert.That(databaseIntegrityErrorsContainer!.Errors.Count, Is.EqualTo(expectedDatabaseIntegrityErrorsData.Length));

        // For each error in the container, verify that there's a matching entry among expected errors data
        foreach (var error in databaseIntegrityErrorsContainer.Errors)
        {
            Assert.That(expectedDatabaseIntegrityErrorsData.Any(expectedData => error.EntityType == expectedData.ExpectedEntityType));
            Assert.That(expectedDatabaseIntegrityErrorsData.Any(expectedData => error.Entity == expectedData.ExpectedEntity));
            Assert.That(expectedDatabaseIntegrityErrorsData.Any(expectedData => error.ErrorMessage == expectedData.ExpectedErrorMessage));

            // Verify that, for the actual error, there's an entry with matching conflict entities among expected errors data:
            // * The length of conflict entities of the error must match the length of expected conflict entities in the entry.
            // * For each conflict entity from the actual error, there must be a matching expected conflict entity among those in the entry.
            Assert.That(expectedDatabaseIntegrityErrorsData.Any(expectedData =>
                error.ConflictEntities.Count() == expectedData.ExpectedConflictEntities.Length
                && error.ConflictEntities.All(confilctEntity =>
                    expectedData.ExpectedConflictEntities.Any(expectedConflictEntity =>
                        confilctEntity.EntityType == expectedConflictEntity.EntityType
                        && confilctEntity.Entity == expectedConflictEntity.Entity
                    )
                )
            ));
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
    public static void MockChacharsDbSet(
        Mock<AsciiPinyinContext> asciiPinyinContextMock,
        params Chachar[] data
    )
    {
        var dataQueryable = data.AsQueryable();
        var chacharsDbSetMock = new Mock<DbSet<Chachar>>();

        _ = chacharsDbSetMock
            .Setup(chacharsDbSet => chacharsDbSet.Find(It.IsAny<object[]>()))
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


        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator);
        _ = asciiPinyinContextMock.Setup(context => context.Chachars).Returns(chacharsDbSetMock.Object);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    public static void MockAlternativesDbSet(
        Mock<AsciiPinyinContext> asciiPinyinContextMock,
        params Alternative[] data
    )
    {
        var dataQueryable = data.AsQueryable();
        var alternativesDbSetMock = new Mock<DbSet<Alternative>>();

        _ = alternativesDbSetMock
            .Setup(alternativesDbSet => alternativesDbSet.Find(It.IsAny<object[]>()))
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

        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator);
        _ = asciiPinyinContextMock.Setup(context => context.Alternatives).Returns(alternativesDbSetMock.Object);
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
