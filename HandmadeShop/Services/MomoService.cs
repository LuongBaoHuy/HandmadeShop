using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using HandmadeShop.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HandmadeShop.Services
{
    public class MomoService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly MomoOptions _opt;
        private readonly ILogger<MomoService> _logger;

        public MomoService(IHttpClientFactory httpFactory, IOptions<MomoOptions> opt, ILogger<MomoService> logger)
        {
            _httpFactory = httpFactory;
            _opt = opt.Value;
            _logger = logger;
        }

        public record MomoCreateResult(bool Success, string? PayUrl, string? QrCodeUrl, string? Message);

        public async Task<MomoCreateResult> CreatePaymentAsync(Models.Order order)
        {
            var requestId = Guid.NewGuid().ToString("N");
            var orderId = $"{_opt.PartnerCode}-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var amount = ((long)Math.Round(order.TotalPrice));
            var orderInfo = $"Thanh toán đơn hàng #{order.Id}";
            var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"orderId={order.Id}"));

            var raw = $"accessKey={_opt.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={_opt.IpnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_opt.PartnerCode}&redirectUrl={_opt.ReturnUrl}&requestId={requestId}&requestType={_opt.RequestType}";
            var signature = Sign(raw, _opt.SecretKey);

            var payload = new
            {
                partnerCode = _opt.PartnerCode,
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = _opt.ReturnUrl,
                ipnUrl = _opt.IpnUrl,
                extraData,
                requestType = _opt.RequestType,
                lang = "vi",
                signature
            };

            var client = _httpFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, _opt.CreateUrl);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var res = await client.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();
            dynamic? json = JsonConvert.DeserializeObject(content);

            if (res.IsSuccessStatusCode && json != null && (int)json.resultCode == 0)
            {
                string payUrl = json.payUrl;
                string? qrCodeUrl = json.qrCodeUrl != null ? (string)json.qrCodeUrl : null;
                return new MomoCreateResult(true, payUrl, qrCodeUrl, "OK");
            }

            _logger.LogError("MoMo create payment failed: {Content}", content);
            string msg = json?.message?.ToString() ?? "MoMo create error";
            return new MomoCreateResult(false, null, null, msg);
        }

        public static string Sign(string raw, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}