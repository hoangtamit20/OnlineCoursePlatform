using OnlineCoursePlatform.DTOs.OrderDtos;

namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class OrderConfirmedDto : OrderInfoDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}