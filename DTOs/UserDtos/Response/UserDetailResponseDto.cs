namespace OnlineCoursePlatform.DTOs.UserDtos.Response
{
    public class UserDetailResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Picture { get; set; }
        public string? Address { get; set; }
        public int CountCoursesPurchased { get; set; }
        public int CountCoursesSold { get; set; }
    }
}