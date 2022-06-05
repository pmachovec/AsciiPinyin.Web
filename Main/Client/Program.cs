using AsciiPinyin.Web.Client;
using AsciiPinyin.Web.Client.Shared;
using AsciiPinyin.Web.Client.Shared.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLocalization();
builder.Services.AddSingleton<JSInteropConsole>();
builder.Services.AddSingleton<JSInteropDOM>();
builder.Services.AddSingleton<SafeLocalization>();

await builder.Build().RunAsync();
