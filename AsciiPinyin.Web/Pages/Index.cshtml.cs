namespace AsciiPinyin.Web.Pages;

using Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;

public class IndexModel : PageModel
{
    // private readonly ILogger<IndexModel> _logger;
    private readonly AsciiPinyinContext _asciiPinyinContext;

    public IndexModel(
        ILogger<IndexModel> logger,
        AsciiPinyinContext asciiPinyinContext) // Automatic constructor DI
    {
        // _logger = logger;
        _asciiPinyinContext = asciiPinyinContext;
    }

    public IEnumerable<Chachar> Chachars { get; private set; } = Enumerable.Empty<Chachar>();

    // ReSharper disable once UnusedMember.Global
    public void OnGet() // Doesn't override anything. It just has the expected name.
    {
        Chachars = _asciiPinyinContext.Chachars;
    }
}
