using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.EntityClient;
using AsciiPinyin.Web.Client.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NLog.Config;
using NLog.Extensions.Logging;

var jsInteropConsoleTarget = new JSInteropConsoleTarget("AsciiPinyin.Web.Client.*");
var builder = WebAssemblyHostBuilder.CreateDefault(args);
_ = builder.Logging.ClearProviders();

_ = builder.Services
    .AddLocalization()
    .AddLogging(loggingBulder =>
        {
            var config = new LoggingConfiguration();
            config.AddRule(
                minLevel: NLog.LogLevel.Info,
                maxLevel: NLog.LogLevel.Fatal,
                target: jsInteropConsoleTarget
            );
            _ = loggingBulder.AddNLog(config);
        }
    )
    .AddSingleton(_ =>
        new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        }
    )
    .AddSingleton<IEntityFormCommons, EntityFormCommons>()
    .AddSingleton<IEntityClient, EntityClient>()
    .AddSingleton<IJSInteropConsole, JSInteropConsole>()
    .AddSingleton<IJSInteropDOM, JSInteropDOM>()
    .AddSingleton<IModalCommons, ModalCommons>();

var host = builder.Build();
var jsInteropConsole = host.Services.GetRequiredService<IJSInteropConsole>();
jsInteropConsoleTarget.JSInteropConsole = jsInteropConsole;

await host.RunAsync();
