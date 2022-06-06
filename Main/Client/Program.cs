using AsciiPinyin.Web.Client;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using AsciiPinyin.Web.Client.Shared.Lokal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLocalization();
builder.Services.AddSingleton<IJSInteropConsole, JSInteropConsole>();
builder.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();
builder.Services.AddSingleton<ILokal, Lokal>();

await builder.Build().RunAsync();
