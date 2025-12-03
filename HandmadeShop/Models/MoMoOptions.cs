namespace HandmadeShop.Models
{
    public class MomoOptions
    {
        public string PartnerCode { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string CreateUrl { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
        public string ReturnUrl { get; set; } = "";
        public string IpnUrl { get; set; } = "";
        public string RequestType { get; set; } = "captureWallet";
    }
}