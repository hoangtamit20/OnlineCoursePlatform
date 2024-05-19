namespace OnlineCoursePlatform.DTOs.CartDtos
{
    public class CartResponseDto
    {
        public string OwnerName { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public List<CartItemInfoDto> CartItemInfoDtos { get; set; } = new List<CartItemInfoDto>();

    }

    public class CartItemInfoDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? Thumbnail { get; set; }
        public decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
    }
}