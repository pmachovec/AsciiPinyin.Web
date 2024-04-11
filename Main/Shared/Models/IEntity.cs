namespace AsciiPinyin.Web.Shared.Models;

public interface IEntity
{
    /*
     * The string type must be used even for single characters.
     * The char type tends to malfunction when sent over HTTP requests.
     */
    string TheCharacter { get; set; }
}
