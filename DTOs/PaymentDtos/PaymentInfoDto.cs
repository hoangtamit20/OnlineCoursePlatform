using Newtonsoft.Json;

namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentInfoDto
    {
        public string PaymentContent { get; set; } = null!;
        public string? PaymentCurrency { get; set; }
        public decimal? RequiredAmount { get; set; }
        [JsonIgnore]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public DateTime? ExpireDate { get; set; } = DateTime.UtcNow.AddMinutes(50);
        public string? PaymentLanguage { get; set; }
        [JsonIgnore]
        public string? MerchantId { get; set; }
        public string PaymentDestinationId { get; set; } = null!;
        public string OrderId { get; set; } = null!;
    }
}