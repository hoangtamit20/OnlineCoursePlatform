using Newtonsoft.Json;

namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentInfoDto
    {
        public string PaymentContent { get; set; } = null!;
        public decimal? RequiredAmount { get; set; }
        public string OrderId { get; set; } = null!;
    }

    public class PaymentInfoRequestDto : PaymentInfoDto
    {
        public string? PaymentCurrency { get; set; } = "VND";
        [JsonIgnore]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public DateTime? ExpireDate { get; set; } = DateTime.UtcNow.AddMinutes(50);
        public string? PaymentLanguage { get; set; } = "VN";
        [JsonIgnore]
        public string? MerchantId { get; set; } = "1";
        public string PaymentDestinationId { get; set; } = "df023f81-8346-4f40-9b12-cca7eb5dec42";
    }
}