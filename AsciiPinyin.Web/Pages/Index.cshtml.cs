using AsciiPinyin.Web.Models;
using AsciiPinyin.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AsciiPinyin.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ChacharJsonService chacharJsonService;
        private IEnumerable<Chachar> chachars = Enumerable.Empty<Chachar>();

        public IEnumerable<Chachar> Chachars { get => chachars; }

        public IndexModel(
            ILogger<IndexModel> logger,
            ChacharJsonService chacharJsonService) // Automatic constructor DI
        {
            _logger = logger;
            this.chacharJsonService = chacharJsonService;
        }

        public void OnGet()
        {
            chachars = chacharJsonService.GetChachars();
        }
    }
}
