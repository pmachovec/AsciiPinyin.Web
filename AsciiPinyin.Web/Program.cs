using AsciiPinyin.Web.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<ChacharJsonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/characters", (context) =>
{
    var chachars = app.Services.GetService<ChacharJsonService>()?.GetChachars();

    if (chachars != null)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var chacharsJson = JsonSerializer.Serialize(chachars, options);
        context.Response.ContentType = "text/plain; charset=utf-8";
        return context.Response.WriteAsync(chacharsJson);
    }

    return context.Response.WriteAsync("");
});

app.Run();
