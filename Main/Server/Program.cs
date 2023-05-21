using AsciiPinyin.Web.Server.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddEntityFrameworkSqlite().AddDbContext<AsciiPinyinContext>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    _ = app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles("/asciipinyin");
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("/asciipinyin/index.html");
app.Run();
