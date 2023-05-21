using AsciiPinyin.Web.Client;
using AsciiPinyin.Web.Client.EntityLoader;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Client.Lokal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddLocalization();
builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IEntityLoader, EntityLoader>();
builder.Services.AddSingleton<IJSInteropConsole, JSInteropConsole>();
builder.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();
builder.Services.AddSingleton<ILokal, Lokal>();
await builder.Build().RunAsync();
