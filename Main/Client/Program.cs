using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.EntityClient;
using AsciiPinyin.Web.Client.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
_ = builder.Services.AddLocalization();

_ = builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

_ = builder.Services.AddSingleton<IEntityFormCommons, EntityFormCommons>();
_ = builder.Services.AddSingleton<IEntityClient, EntityClient>();
_ = builder.Services.AddSingleton<IJSInteropConsole, JSInteropConsole>();
_ = builder.Services.AddSingleton<IJSInteropDOM, JSInteropDOM>();
_ = builder.Services.AddSingleton<IModalCommons, ModalCommons>();

await builder.Build().RunAsync();
