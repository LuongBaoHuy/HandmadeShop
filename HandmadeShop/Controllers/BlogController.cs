using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeShop.Controllers
{
    public class BlogController : BaseController
    {
        private readonly ILogger<BlogController> _logger;

        public BlogController(HandmadeShopContext context, ILogger<BlogController> logger) : base(context)
        {
            _logger = logger;
        }
        public IActionResult Blog()
        {
            return View();
        }
    }
}
