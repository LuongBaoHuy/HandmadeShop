using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HandmadeShop.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HandmadeShop.Controllers
{
    public class BaseController : Controller
    {
        protected HandmadeShopContext db;

        public BaseController(HandmadeShopContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    ViewBag.UserAvatar = user.ProfileImageUrl ?? "/images/default-avatar.png";
                    ViewBag.UserName = user.Username;
                    ViewBag.FullName = user.FullName;
                    var cartItems = db.CartItems
                        .Where(c => c.UserId == user.Id)
                        .Include(c => c.Product)
                        .ToList();
                    ViewBag.CartItems = cartItems;
                    ViewBag.CartTotal = cartItems.Sum(c => (c.Product?.Price ?? 0) * c.Quantity);
                    ViewBag.CartCount = cartItems.Sum(c => c.Quantity);
                }
                else
                {
                    ViewBag.CartCount = 0;
                }
            }
            else
            {
                ViewBag.CartCount = 0;
            }
        }
    }
} 