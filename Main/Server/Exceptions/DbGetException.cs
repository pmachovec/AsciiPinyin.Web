namespace AsciiPinyin.Web.Server.Exceptions;

public class DbGetException(Exception innerException) : Exception(innerException.Message, innerException)
{
}
