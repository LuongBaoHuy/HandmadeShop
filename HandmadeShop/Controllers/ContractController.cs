using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeShop.Controllers
{
    public class ContractController : BaseController
    {
        private readonly ILogger<ContractController> _logger;

        public ContractController(HandmadeShopContext context, ILogger<ContractController> logger) : base(context)
        {
            _logger = logger;
        }
        public IActionResult Contract()
        {
            return View();
        }
    }
}
