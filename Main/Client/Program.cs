using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddLocalization();
builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddSingleton<IEntityLoader, EntityLoader>();
builder.Services.AddSingleton<IJSInteropConsole, JSInteropConsole>();
builder.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();

await builder.Build().RunAsync();
