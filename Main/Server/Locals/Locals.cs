namespace AsciiPinyin.Web.Server.Locals;

internal sealed class Locals(string _nLogConfigYamlPath) : ILocals
{
    public string NLogConfigYamlPath => _nLogConfigYamlPath;
}
