using NLog;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

namespace AsciiPinyin.Web.Client.JSInterop;

[Target(nameof(JSInteropConsoleTarget))]
public sealed class JSInteropConsoleTarget : TargetWithLayout
{
    public IJSInteropConsole? JSInteropConsole { private get; set; }

    protected override void Write(LogEventInfo logEvent)
    {
        if (JSInteropConsole is null)
        {
            throw new InvalidOperationException($"{nameof(JSInteropConsole)} is null");
        }

        var logMessage = Layout.Render(logEvent);

        if (logEvent.Level <= LogLevel.Info)
        {
            JSInteropConsole.ConsoleInfo(logMessage);
        }
        else if (logEvent.Level == LogLevel.Warn)
        {
            JSInteropConsole.ConsoleWarning(logMessage);
        }
        else
        {
            JSInteropConsole.ConsoleError(logMessage);
        }
    }
}
