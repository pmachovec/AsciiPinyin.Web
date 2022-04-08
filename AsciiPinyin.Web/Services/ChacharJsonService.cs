using AsciiPinyin.Web.Models;
using System.Text.Json;

namespace AsciiPinyin.Web.Services;

public class ChacharJsonService
{
    public ChacharJsonService(IWebHostEnvironment webHostEnvironment)
    {
        WebHostEnvironment = webHostEnvironment;
    }

    /*
     * Provider of information about the web hosting environment an application is running in.
     * It's another service that we get it automatically from .NET in the constructor (dependency injection).
     */
    private IWebHostEnvironment WebHostEnvironment { get; }

    // Path to the database file retrieved programmatically from the WebHostEnvironment service.
    private string ChacharJsonFilePath =>
        Path.Combine( // Path as string creator
            WebHostEnvironment.WebRootPath, // Path to the 'wwwroot' folder
            "database",
            "asciipinyin.json");

    /**
     * Transfers the Json database file (which must be in the expected location) to an IEnumerable of Chachars.
     */
    public IEnumerable<Chachar> GetChachars()
    {
        using var fileReader = File.OpenText(ChacharJsonFilePath);

        var chachars = JsonSerializer.Deserialize<Chachar[]>(
            fileReader.ReadToEnd(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return chachars ?? Enumerable.Empty<Chachar>();
    }
}
