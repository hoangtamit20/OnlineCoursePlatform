namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentLinkDto
    {
        /// <summary>
        /// Mã giao dịch yêu cầu thanh toán
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? PaymentUrl { get; set; } = string.Empty;
    }
}