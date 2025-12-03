using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeShop.Controllers
{
    public class Terms_conditionsController : BaseController
    {
        private readonly ILogger<Terms_conditionsController> _logger;

        public Terms_conditionsController(HandmadeShopContext context, ILogger<Terms_conditionsController> logger) : base(context)
        {
            _logger = logger;
        }
        public IActionResult Terms_conditions()
        {
            return View();
        }
    }
}
