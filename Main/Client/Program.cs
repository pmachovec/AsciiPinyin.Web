using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NLog.Extensions.Logging;
using LoggingConfiguration = NLog.Config.LoggingConfiguration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

_ = builder.Logging
    .ClearProviders()
    .AddNLog();

_ = builder.Services
    .AddLocalization()
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
    .AddSingleton<INLogConfigClient, NLogConfigClient>()
    .AddSingleton<IModalCommons, ModalCommons>();

var host = builder.Build();

var nlogConfig = await host.Services.GetRequiredService<INLogConfigClient>().GetNLogConfigAsync(CancellationToken.None);
var jsInteropConsole = host.Services.GetRequiredService<IJSInteropConsole>();

if (nlogConfig is not null)
{
    foreach (var jsInteropConsoleTarget in nlogConfig.AllTargets.OfType<JSInteropConsoleTarget>())
    {
        jsInteropConsoleTarget.JSInteropConsole = jsInteropConsole;
    };
}
else
{
    jsInteropConsole.ConsoleWarning("Loading NLog configuration from the server failed, using built-in configuration.");
    nlogConfig = new LoggingConfiguration();

    var jsInteropConsoleTarget = new JSInteropConsoleTarget
    {
        Name = "AsciiPinyin.Web.Client.*"
    };

    nlogConfig.AddRule(
        minLevel: NLog.LogLevel.Info,
        maxLevel: NLog.LogLevel.Fatal,
        target: jsInteropConsoleTarget
    );

    jsInteropConsoleTarget.JSInteropConsole = jsInteropConsole;
}

NLog.LogManager.Configuration = nlogConfig;
await host.RunAsync();
