using AsciiPinyin.Web.Server.Pages;
using AsciiPinyin.Web.Server.Data;
using Index = AsciiPinyin.Web.Client.Pages.Index.Index;

var builder = WebApplication.CreateBuilder(args);
_ = builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
_ = builder.Services.AddControllers();
_ = builder.Services.AddEntityFrameworkSqlite().AddDbContext<AsciiPinyinContext>();

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

_ = app.UseHttpsRedirection();
_ = app.UseStaticFiles();
_ = app.UseAntiforgery();
_ = app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Index).Assembly);
_ = app.MapControllers();

app.Run();
