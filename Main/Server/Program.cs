using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Controllers.ActionFilters;
using AsciiPinyin.Web.Server.Data;
using AsciiPinyin.Web.Server.Locals;
using AsciiPinyin.Web.Server.Middleware;
using AsciiPinyin.Web.Server.Pages;
using AsciiPinyin.Web.Server.Validation;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Globalization;
using System.Reflection;
using System.Text;
using Index = AsciiPinyin.Web.Client.Pages.Index;

Console.OutputEncoding = Encoding.UTF8;
var builder = WebApplication.CreateBuilder(args);
var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var nLogConfigYamlPath = $@"{assemblyLocation}/{StringConstants.NLOG_CONFIG_YAML_IN_FOLDER}";

_ = builder.Logging.ClearProviders().AddNLog();

_ = builder.Configuration.AddYamlFile(
    nLogConfigYamlPath,
    optional: false,
    reloadOnChange: true
);

_ = builder.Services
    .AddDbContext<AsciiPinyinContext>(optionsBuilder => optionsBuilder.UseSqlite("Data Source=Data/asciipinyin.sqlite"))
    .AddLocalization()
    .AddScoped<AlternativeGetFilter>()
    .AddScoped<AlternativePostFilter>()
    .AddScoped<AlternativePostDeleteFilter>()
    .AddScoped<ChacharGetFilter>()
    .AddScoped<ChacharPostFilter>()
    .AddScoped<ChacharPostDeleteFilter>()
    .AddScoped<IPostFilterCommons, PostFilterCommons>() // Doesn't work as singleton, must be scoped, because the consumed DB context is also scoped.
    .AddScoped<IEntityControllerCommons, EntityControllerCommons>()
    .AddSingleton<ILocals>(_ => new Locals(nLogConfigYamlPath))
    .Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true) // Replaced by AsciiPinyinModelStateInvalidFilter.
    .AddControllers(options => options.Filters.Add<ModelStateInvalidFilter>())
    .AddMvcOptions(options => options.ModelMetadataDetailsProviders.Add(new ControllerValidationMetadataProvider()));

_ = builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();
_ = app.UsePathBase($"/{ApiNames.BASE}");

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    _ = app.UseExceptionHandler("/Error", createScopeForErrors: true);
    _ = app.UseHsts();
}

app.UseRequestLocalization(options =>
    {
        const string EN_US = "en-US";
        const string CS_CZ = "cs-CZ";

        var supportedCultures = new List<CultureInfo>
        {
            new(EN_US),
            new(CS_CZ),
        };

        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders.Clear();

        options.RequestCultureProviders.Insert(
            0,
            new CustomRequestCultureProvider(async context =>
                {
                    var userLanguages = context.Request.Headers.AcceptLanguage.ToString();
                    var primaryLanguage = userLanguages.Split(',').FirstOrDefault() ?? EN_US;

                    if (primaryLanguage == "cs")
                    {
                        primaryLanguage = CS_CZ;
                    }

                    var userCulture = new CultureInfo(primaryLanguage);
                    return await Task.FromResult(new ProviderCultureResult(userCulture?.Name ?? supportedCultures[0].Name));
                }
            )
        );
    }
);

_ = app.UseHttpsRedirection()
    .UseStaticFiles()
    .UseAntiforgery()
    .UseMiddleware<LogRequestMiddleware>()
    .UseMiddleware<VerifyUserAgentMiddleware>();

_ = app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Index).Assembly);

_ = app.MapControllers();

app.Run();
