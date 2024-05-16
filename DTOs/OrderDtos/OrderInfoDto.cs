using OnlineCoursePlatform.Data.Entities.Order;

namespace OnlineCoursePlatform.DTOs.OrderDtos
{
    public class OrderInfoDto
    {
        public string Id { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public string ShipName { get; set; } = null!;

        public string ShipAddress { get; set; } = null!;

        public string? ShipEmail { get; set; }

        public string ShipPhoneNumber { get; set; } = null!;

        public OrderStatus Status { get; set; }

        public decimal TotalPrice { get; set; }
    }
}