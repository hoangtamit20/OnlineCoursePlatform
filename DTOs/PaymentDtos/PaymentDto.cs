namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentDto
    {
        public string Id { get; set; } = null!;
        public string PaymentContent { get; set; } = string.Empty;
        public string PaymentCurrency { get; set; } = string.Empty;
        public string PaymentRefId { get; set; } = string.Empty;
        public decimal? RequiredAmount { get; set; }
        public DateTime? PaymentDate { get; set; } = DateTime.Now;
        public DateTime? ExpireDate { get; set; }
        public string? PaymentLanguage { get; set; } = string.Empty;
        public string MerchantId { get; set; } = null!;
        public string PaymentDestinationId { get; set; } = null!;
        public PaymentStatus PaymentStatus { get; set; }
        public decimal? PaidAmount { get; set; }
    }


    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Cancelled
    }
}