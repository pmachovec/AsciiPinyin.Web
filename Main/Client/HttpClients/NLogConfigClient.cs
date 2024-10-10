using AsciiPinyin.Web.Client.JSInterop;
using AsciiPinyin.Web.Shared.Constants;
using NLog.Config;
using System.Xml;

namespace AsciiPinyin.Web.Client.HttpClients;

public class NLogConfigClient(
    HttpClient _httpClient,
    IJSInteropConsole _jsInteropConsole
) : INLogConfigClient
{
    public async Task<LoggingConfiguration?> GetNLogConfigAsync(CancellationToken cancellationToken)
    {
        var nlogConfigContent = await _httpClient.GetStringAsync(ApiNames.NLOGCONFIG, cancellationToken);

        if (string.IsNullOrEmpty(nlogConfigContent))
        {
            _jsInteropConsole.ConsoleWarning("NLog configuration from the server is empty.");
            return null;
        }

        try
        {
            var xmlReader = XmlReader.Create(new StringReader(nlogConfigContent));
            return new XmlLoggingConfiguration(xmlReader);
        }
        catch (Exception ex)
        {
            _jsInteropConsole.ConsoleError("Parsing of NLog configuration from the server failed.");
            _jsInteropConsole.ConsoleError(ex);
            return null;
        }
    }
}
