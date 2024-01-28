namespace OnlineCoursePlatform.DTOs.UserDtos.Response
{
    public class UserBaseInfoResponseDto
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Picture { get; set; }
    }
}