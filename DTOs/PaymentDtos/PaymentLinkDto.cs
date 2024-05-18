namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentLinkDto
    {
        /// <summary>
        /// Mã giao dịch yêu cầu thanh toán
        /// </summary>
        public string PaymentId { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public string? PaymentUrl { get; set; } = string.Empty;
    }
}