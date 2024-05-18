namespace OnlineCoursePlatform.DTOs.PaymentDtos.MerchantDtos
{
    public class CreateMerchantRequestDto
    {
        public string? MerchantName { get; set; }
        public string? MerchantWebLink { get; set; }
        public string? MerchantIpnUrl { get; set; }
        public string? MerchantReturnUrl { get; set; }
        public string? SecretKey { get; set; }
        public bool? IsActive { get; set; }
    }
}