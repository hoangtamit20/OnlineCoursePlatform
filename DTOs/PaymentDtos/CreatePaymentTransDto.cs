namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class CreatePaymentTransDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string? TranMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? TranPayload { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? TranStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? TranAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TranDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? PaymentId { get; set; } = null!;

    }
}