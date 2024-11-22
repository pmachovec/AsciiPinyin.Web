using AsciiPinyin.Web.Client.Commons;
using AsciiPinyin.Web.Client.HttpClients;
using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NLog.Config;
using NLog.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var httpClient = new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

Exception? loadNlogConfigException = null;

try
{
    using var stream = await httpClient.GetStreamAsync(ApiNames.NLOGCONFIG);
    using var fileStream = new FileStream(StringConstants.NLOG_CONFIG_YAML, FileMode.CreateNew);
    await stream.CopyToAsync(fileStream);
}
catch (Exception e)
{
    loadNlogConfigException = e;
}

_ = builder.Logging.ClearProviders().AddNLog();

_ = builder.Configuration.AddYamlFile(
    StringConstants.NLOG_CONFIG_YAML,
    optional: false,
    reloadOnChange: true
);

_ = builder.Services
    .AddLocalization()
    .AddSingleton(_ => httpClient)
    .AddSingleton<IEntityFormCommons, EntityFormCommons>()
    .AddSingleton<IEntityClient, EntityClient>()
    .AddSingleton<IJSInteropConsole, JSInteropConsole>()
    .AddSingleton<IJSInteropDOM, JSInteropDOM>()
    .AddSingleton<IModalCommons, ModalCommons>();

var host = builder.Build();
var jsInteropConsole = host.Services.GetRequiredService<IJSInteropConsole>();

if (loadNlogConfigException is not null)
{
    jsInteropConsole.ConsoleError(loadNlogConfigException);
}

if (loadNlogConfigException is not null || !File.Exists(StringConstants.NLOG_CONFIG_YAML))
{
    jsInteropConsole.ConsoleWarning($"Failed to load the NLog configuration file {StringConstants.NLOG_CONFIG_YAML} from the server, using built-in configuration.");
    var nlogConfig = new LoggingConfiguration();

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
    NLog.LogManager.Configuration = nlogConfig;
}

await host.RunAsync();
