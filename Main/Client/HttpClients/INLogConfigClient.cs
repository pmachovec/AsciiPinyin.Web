using NLog.Config;

namespace AsciiPinyin.Web.Client.HttpClients;

public interface INLogConfigClient
{
    Task<LoggingConfiguration?> GetNLogConfigAsync(CancellationToken cancellationToken);
}
