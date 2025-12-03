using HandmadeShop.Models;
using HandmadeShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions; // <-- thêm

namespace HandmadeShop.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly MomoService _momo;
        private readonly MomoOptions _opt;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(HandmadeShopContext ctx, MomoService momo, IOptions<MomoOptions> opt, ILogger<PaymentController> logger)
            : base(ctx)
        {
            _momo = momo;
            _opt = opt.Value;
            _logger = logger;
        }

        // Helper: Lấy OrderId nội bộ từ extraData hoặc từ momoOrderId (ví dụ "MOMO-3023-17558...")
        private static bool TryResolveOrderId(string? momoOrderId, string? extraData, out int orderId)
        {
            orderId = 0;

            // 1) Ưu tiên extraData: Base64("orderId=3023")
            if (!string.IsNullOrEmpty(extraData))
            {
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(extraData));
                    var val = decoded.Split('=', 2).LastOrDefault();
                    if (int.TryParse(val, out orderId)) return true;
                }
                catch { /* ignore */ }
            }

            // 2) Fallback: tách số đầu tiên trong momoOrderId (MOMO-3023-...)
            if (!string.IsNullOrEmpty(momoOrderId))
            {
                var m = Regex.Match(momoOrderId, @"\d+");
                if (m.Success && int.TryParse(m.Value, out orderId)) return true;
            }

            return false;
        }

        // Tạo yêu cầu và hiển thị QR
        [HttpGet]
        public async Task<IActionResult> Momo(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity!.Name);
            var order = db.Orders.FirstOrDefault(o => o.Id == id && o.UserId == user!.Id);
            if (order == null) return NotFound();

            if (order.IsPaid)
            {
                return RedirectToAction("OrderDetail", "Checkout", new { id = order.Id }); // changed
            }

            if (order.TotalPrice <= 0)
            {
                order.IsPaid = true;
                db.SaveChanges();
                return RedirectToAction("OrderDetail", "Checkout", new { id = order.Id }); // changed
            }

            var result = await _momo.CreatePaymentAsync(order);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View("Momo", new MomoViewModel { OrderId = order.Id, Amount = order.TotalPrice, Error = result.Message });
            }

            return View("Momo", new MomoViewModel
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                QrCodeUrl = result.QrCodeUrl,
                PayUrl = result.PayUrl
            });
        }

        // Return URL khi user quay lại từ MoMo
        [AllowAnonymous]
        [HttpGet("payment/momo-return")]
        public IActionResult MomoReturn()
        {
            var q = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            try
            {
                var extraData = q.GetValueOrDefault("extraData");
                var momoOrderId = q.GetValueOrDefault("orderId");

                if (!TryResolveOrderId(momoOrderId, extraData, out var orderId))
                    return BadRequest("Invalid order");

                var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null) return NotFound();

                var resultCode = q.GetValueOrDefault("resultCode");
                if (resultCode == "0")
                {
                    order.IsPaid = true;
                    db.SaveChanges();
                    TempData["OrderSuccess"] = "Thanh toán thành công.";
                    return RedirectToAction("OrderDetail", "Checkout", new { id = orderId }); // changed
                }

                TempData["OrderError"] = "Thanh toán không thành công.";
                return RedirectToAction("OrderDetail", "Checkout", new { id = orderId }); // unchanged target page
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MomoReturn error");
                return BadRequest();
            }
        }

        // IPN từ MoMo (server-to-server) để xác nhận thanh toán
        [AllowAnonymous]
        [HttpPost("payment/momo-ipn")]
        public IActionResult MomoIpn([FromBody] MomoIpnRequest body)
        {
            try
            {
                if (!TryResolveOrderId(body.orderId, body.extraData, out var orderId))
                    return Ok(new { resultCode = 5, message = "Invalid order" });

                var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null) return Ok(new { resultCode = 5, message = "Order not found" });

                if (body.resultCode == 0)
                {
                    order.IsPaid = true;
                    db.SaveChanges();
                    return Ok(new { resultCode = 0, message = "Confirm Success" });
                }
                return Ok(new { resultCode = 5, message = "Payment Failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Momo IPN error: {Body}", JsonConvert.SerializeObject(body));
                return Ok(new { resultCode = 5, message = "Server Error" });
            }
        }

        // Trang QR sẽ poll trạng thái đơn theo DB (IPN sẽ cập nhật DB)
        [HttpGet]
        public IActionResult OrderPaidStatus(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity!.Name);
            var order = db.Orders.AsNoTracking().FirstOrDefault(o => o.Id == id && o.UserId == user!.Id);
            if (order == null) return Json(new { ok = false });
            return Json(new { ok = true, isPaid = order.IsPaid });
        }
    }

    public class MomoViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? PayUrl { get; set; }
        public string? Error { get; set; }
    }

    public class MomoIpnRequest
    {
        public string partnerCode { get; set; } = "";
        public string orderId { get; set; } = "";
        public string requestId { get; set; } = "";
        public long amount { get; set; }
        public string orderInfo { get; set; } = "";
        public string orderType { get; set; } = "";
        public long transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; } = "";
        public string payType { get; set; } = "";
        public long responseTime { get; set; }
        public string? extraData { get; set; }
        public string signature { get; set; } = "";
    }
}