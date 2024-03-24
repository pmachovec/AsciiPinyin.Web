using AsciiPinyin.Web.Server.Pages;
using AsciiPinyin.Web.Server.Data;
using Index = AsciiPinyin.Web.Client.Pages.Index.Index;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
_ = builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
_ = builder.Services.AddControllers();
_ = builder.Services.AddEntityFrameworkSqlite().AddDbContext<AsciiPinyinContext>();
_ = builder.Services.AddLocalization();

var app = builder.Build();
_ = app.UsePathBase("/asciipinyin");

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
        }));
});

_ = app.UseHttpsRedirection();
_ = app.UseStaticFiles();
_ = app.UseAntiforgery();
_ = app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Index).Assembly);
_ = app.MapControllers();

app.Run();
