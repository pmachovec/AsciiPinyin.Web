using Asciipinyin.Web.Server.Test.Constants;
using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Middleware;
using AsciiPinyin.Web.Server.Test.Constants;
using AsciiPinyin.Web.Server.Validation;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Test.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace AsciiPinyin.Web.Server.Test.Commons;

[
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "With static calls, each would need to explicitly specify generic types. With the current instance approach, generics are fixed per instance."
    ),
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079:Remove unnecessary suppression",
        Justification = "The CA1822 suppression is marked as unnecessary, which is not true."
    )
]
internal class EntityControllerTestCommons<T1, T2>(
    string _path,
    string _pathDelete,
    Mock<AsciiPinyinContext> _asciiPinyinContextMock
) where T1 : ControllerBase, IEntityController where T2 : IEntity
{
    private const string TEST = "test";
    private const string TEST_DB = "test_db";

    private readonly Mock<ILogger<T1>> _entityControllerLoggerMock = new();
    private readonly Mock<ILogger<VerifyUserAgentMiddleware>> _userAgentVerifierLoggerMock = new();
    private readonly Mock<DatabaseFacade> _databaseFacadeMock = new(_asciiPinyinContextMock.Object);
    private readonly Mock<IDbContextTransaction> _dbContextTransactionMock = new();

    public async Task<IHost> SetUpHost()
    {
        _ = _databaseFacadeMock.Setup(m => m.BeginTransaction()).Returns(_dbContextTransactionMock.Object);
        var testCategory = TestContext.CurrentContext.Test?.Properties["Category"].FirstOrDefault();
        IHost host;

        switch (testCategory)
        {
            case TestCategories.DB_CONTEXT_MOCK:
                _ = _asciiPinyinContextMock.Setup(context => context.Database).Returns(_databaseFacadeMock.Object);
                MockChacharsDbSet(_asciiPinyinContextMock);
                MockAlternativesDbSet(_asciiPinyinContextMock);
                host = await GetHostAsync(_asciiPinyinContextMock.Object, CancellationToken.None);
                break;

            case TestCategories.DB_CONTEXT_MOCK_ERROR_ALTERNATIVES:
                _ = _asciiPinyinContextMock.Setup(context => context.Database).Returns(_databaseFacadeMock.Object);
                _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.Alternatives).Throws(new InvalidOperationException());
                MockChacharsDbSet(_asciiPinyinContextMock);
                host = await GetHostAsync(_asciiPinyinContextMock.Object, CancellationToken.None);
                break;

            case TestCategories.DB_CONTEXT_MOCK_ERROR_CHACHARS:
                _ = _asciiPinyinContextMock.Setup(context => context.Database).Returns(_databaseFacadeMock.Object);
                _ = _asciiPinyinContextMock.Setup(asciiPinyinContext => asciiPinyinContext.Chachars).Throws(new InvalidOperationException());
                MockAlternativesDbSet(_asciiPinyinContextMock);
                host = await GetHostAsync(_asciiPinyinContextMock.Object, CancellationToken.None);
                break;

            default:
                host = await GetHostAsync(CancellationToken.None);
                break;
        }

        return host;
    }

    public void TearDown(AsciiPinyinContext asciiPinyinContext, IHost host)
    {
        _asciiPinyinContextMock.Reset();
        _ = asciiPinyinContext.Database?.EnsureDeleted();
        host.Dispose();
    }

    public async Task<HttpResponseMessage> GetAsync(IHost host, CancellationToken cancellationToken) =>
        await GetTestClient(host).GetAsync(_path, cancellationToken);

    public async Task<HttpResponseMessage> PostAsync(IHost host, T2 entity, CancellationToken cancellationToken) =>
        await GetTestClient(host).PostAsJsonAsync(_path, entity, cancellationToken: cancellationToken);

    public async Task<HttpResponseMessage> PostDeleteAsync(IHost host, T2 entity, CancellationToken cancellationToken) =>
        await GetTestClient(host).PostAsJsonAsync(_pathDelete, entity, cancellationToken: cancellationToken);

    public void AddToContextAndSave(
        AsciiPinyinContext asciiPinyinContext,
        params IEntity[] entities
    )
    {
        foreach (var entity in entities)
        {
            _ = asciiPinyinContext.Add(entity);
        }

        _ = asciiPinyinContext.SaveChanges();
    }

    public async Task NoUserAgentHeaderTestAsync(HttpResponseMessage? response)
    {
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.StatusCode, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo(Errors.USER_AGENT_MISSING));
    }

    public void InternalServerErrorTest(HttpResponseMessage? response)
    {
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.StatusCode, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    public async Task GetAllEntitiesOkTestAsync(
        HttpResponseMessage? response,
        params T2[] expectedEntities
    )
    {
        TestResponseStatusCode(response, HttpStatusCode.OK);

        var entities = await response!.Content.ReadFromJsonAsync<IEnumerable<T2>>();
        Assert.That(entities, Is.Not.Null);
        Assert.That(entities, Is.EquivalentTo(expectedEntities));
    }

    public async Task PostWrongFieldTestAsync(
        HttpResponseMessage? response,
        string fieldName,
        CancellationToken cancellationToken,
        params string[] expectedErrors
    )
    {
        var contentObject = await TestAndGetContentObjectAsync(response, HttpStatusCode.BadRequest, cancellationToken);
        var fieldErrors = contentObject[JsonPropertyNames.ERRORS]?[fieldName]?.AsArray()?.Select(e => e!.GetValue<string>());
        Assert.That(fieldErrors, Is.Not.Null);
        Assert.That(fieldErrors, Is.EquivalentTo(expectedErrors));
    }

    public async Task PostBadRequestTestAsync(
        HttpResponseMessage? response,
        CancellationToken cancellationToken,
        params string[] expectedErrors
    ) => await TestErrorsArrayAsync(response, HttpStatusCode.BadRequest, expectedErrors, cancellationToken);

    public async Task PostConflictTestAsync(
        HttpResponseMessage? response,
        CancellationToken cancellationToken,
        params string[] expectedErrors
    ) => await TestErrorsArrayAsync(response, HttpStatusCode.Conflict, expectedErrors, cancellationToken);

    public void PostOkTest(HttpResponseMessage? response)
    {
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.StatusCode, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    public string GetEntityUnknownErrorMessage(string entityType, params string[] fieldNames) =>
        $"combination of fields '{string.Join(" + ", fieldNames)}' does not identify an existing {entityType}";

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    private void MockChacharsDbSet(Mock<AsciiPinyinContext> asciiPinyinContextMock)
    {
        var data = new HashSet<Chachar>();
        var chacharsDbSetMock = new Mock<DbSet<Chachar>>();

        _ = chacharsDbSetMock
            .Setup(chacharsDbSet => chacharsDbSet.Find(It.IsAny<object[]>()))
            .Returns((object[] parameters) =>
            {
                if (
                    parameters.Length == 3
                    && parameters[0] is string theCharacter
                    && parameters[1] is string pinyin
                    && parameters[2] is short tone
                )
                {
                    return data.FirstOrDefault(d =>
                        d.TheCharacter == theCharacter
                        && d.Pinyin == pinyin
                        && d.Tone == tone
                    );
                }

                return null;
            });

        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        _ = chacharsDbSetMock.As<IQueryable<Chachar>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator);

        _ = asciiPinyinContextMock
            .Setup(asciiPinyinContext => asciiPinyinContext.Chachars)
            .Returns(chacharsDbSetMock.Object);

        _ = asciiPinyinContextMock
            .Setup(asciiPinyinContext => asciiPinyinContext.Add(It.IsAny<Chachar>()))
            .Callback((Chachar chachar) => data.Add(chachar));
    }

    // Forget about unification, you can't create DbSet with a generic type.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Conditional expression looks terrible on that 'if' statement."
    )]
    private void MockAlternativesDbSet(Mock<AsciiPinyinContext> asciiPinyinContextMock)
    {
        var data = new HashSet<Alternative>();
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
                    && parameters[3] is short originalTone
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
            });

        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        _ = alternativesDbSetMock.As<IQueryable<Alternative>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator);

        _ = asciiPinyinContextMock
            .Setup(asciiPinyinContext => asciiPinyinContext.Alternatives)
            .Returns(alternativesDbSetMock.Object);

        _ = asciiPinyinContextMock
            .Setup(asciiPinyinContext => asciiPinyinContext.Add(It.IsAny<Alternative>()))
            .Callback((Alternative alternative) => data.Add(alternative));
    }

    private HttpClient GetTestClient(IHost host)
    {
        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add(RequestHeaderKeys.USER_AGENT, TEST);
        return client;
    }

    private async Task<IHost> GetHostAsync(AsciiPinyinContext asciiPinyinContext, CancellationToken cancellationToken)
    {
        var hostBuilder = GetHostBuilder();

        _ = hostBuilder.ConfigureServices(services =>
            services.AddScoped(_ => asciiPinyinContext)
        );

        return await hostBuilder.StartAsync(cancellationToken);
    }

    private async Task<IHost> GetHostAsync(CancellationToken cancellationToken)
    {
        var hostBuilder = GetHostBuilder();

        _ = hostBuilder.ConfigureServices(services =>
            services.AddDbContext<AsciiPinyinContext>(optionsBuilder =>
                optionsBuilder
                    .UseInMemoryDatabase(TEST_DB)
                    .ConfigureWarnings(warningsConfigurationBuilder => warningsConfigurationBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            )
        );

        return await hostBuilder.StartAsync(cancellationToken);
    }

    private IHostBuilder GetHostBuilder() =>
        new HostBuilder().ConfigureWebHost(webBuilder =>
        {
            _ = webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    _ = services
                        .AddSingleton(_entityControllerLoggerMock.Object)
                        .AddSingleton(_userAgentVerifierLoggerMock.Object)
                        .AddScoped<AlternativeGetFilter>()
                        .AddScoped<AlternativePostFilter>()
                        .AddScoped<AlternativePostDeleteFilter>()
                        .AddScoped<ChacharGetFilter>()
                        .AddScoped<ChacharPostFilter>()
                        .AddScoped<ChacharPostDeleteFilter>()
                        .AddScoped<IPostFilterCommons, PostFilterCommons>()
                        .AddScoped<IEntityControllerCommons, EntityControllerCommons>()
                        .Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true)
                        .AddControllers(options => options.Filters.Add<ModelStateInvalidFilter>())
                        .AddMvcOptions(options => options.ModelMetadataDetailsProviders.Add(new ControllerValidationMetadataProvider()))
                        .AddApplicationPart(typeof(T1).Assembly);
                })
                .Configure(app =>
                    _ = app
                        .UseMiddleware<VerifyUserAgentMiddleware>()
                        .UseRouting()
                        .UseEndpoints(endpoints => endpoints.MapControllers())
                );
        });

    private async Task TestErrorsArrayAsync(
        HttpResponseMessage? response,
        HttpStatusCode expectedStatusCode,
        string[] expectedErrors,
        CancellationToken cancellationToken
    )
    {
        var contentObject = await TestAndGetContentObjectAsync(response, expectedStatusCode, cancellationToken);
        var errors = contentObject[JsonPropertyNames.ERRORS]?.AsArray()?.Select(e => e!.GetValue<string>());
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Is.EquivalentTo(expectedErrors));
    }

    private async Task<JsonObject> TestAndGetContentObjectAsync(
        HttpResponseMessage? response,
        HttpStatusCode expectedStatusCode,
        CancellationToken cancellationToken
    )
    {
        TestResponseStatusCode(response, expectedStatusCode);

        var contentString = await response!.Content.ReadAsStringAsync(cancellationToken);
        Assert.That(contentString, Is.Not.Null);
        Assert.That(contentString.Length, Is.GreaterThan(0));

        var contentObject = JsonNode.Parse(contentString)!.AsObject();
        Assert.That(contentObject, Is.Not.Null);
        return contentObject;
    }

    private void TestResponseStatusCode(
        HttpResponseMessage? response,
        HttpStatusCode expectedStatusCode
    )
    {
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.StatusCode, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
        Assert.That(response.Content, Is.Not.Null);
    }
}
