using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
_ = builder.Services.AddLocalization();
_ = builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
_ = builder.Services.AddSingleton<IEntityLoader, EntityLoader>();
_ = builder.Services.AddSingleton<IJSInteropConsole, JSInteropConsole>();
_ = builder.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();

await builder.Build().RunAsync();
