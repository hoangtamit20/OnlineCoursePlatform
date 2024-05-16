namespace OnlineCoursePlatform.Configurations
{
    public class VNPayConfig
    {
        public static string ConfigName => "VnPay";
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
    }
}