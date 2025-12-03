using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeShop.Controllers
{
    public class AboutController : BaseController
    {
        private readonly ILogger<AboutController> _logger;

        public AboutController(HandmadeShopContext context, ILogger<AboutController> logger) : base(context)
        {
            _logger = logger;
        }
        public IActionResult About()
        {
            return View();
        }
    }
}
