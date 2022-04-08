namespace AsciiPinyin.Web.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ChacharJsonService _chacharJsonService;

    public IndexModel(
        ILogger<IndexModel> logger,
        ChacharJsonService chacharJsonService) // Automatic constructor DI
    {
        _logger = logger;
        _chacharJsonService = chacharJsonService;
    }

    public IEnumerable<Chachar> Chachars { get; private set; } = Enumerable.Empty<Chachar>();

    // ReSharper disable once UnusedMember.Global
    public void OnGet() // Doesn't override anything. It just has the expected name.
    {
        Chachars = _chacharJsonService.GetChachars();
    }
}
