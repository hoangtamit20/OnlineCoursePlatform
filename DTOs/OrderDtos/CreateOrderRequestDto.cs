using OnlineCoursePlatform.Data.Entities.Order;

namespace OnlineCoursePlatform.DTOs.OrderDtos
{
    public class CreateOrderRequestDto
    {
        public List<int> CourseIds { get; set; } = new();
    }

    public class CreateOrderResponseDto : OrderInfoDto
    {
        
    }

    public class OrderInfoResponseDto
    {
        public string OrderId { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; internal set; }
    }

    public class OrderInfoForAdminDto : OrderInfoResponseDto
    {
        public OrderOwnerInfoDto OrderOwnerInfoDto { get; set; } = null!;
    }

    public class OrderOwnerInfoDto
    {
        public string UserId { get; set; } = null!;
        public string OrdererName { get; set; } = null!;
        public string? Picture { get; set; }
    }

    public class OrderDetailDto
    {
        public string OrderId { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public string? OrdererName { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int CourseId { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}