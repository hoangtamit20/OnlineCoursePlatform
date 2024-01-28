namespace OnlineCoursePlatform.DTOs.UserDtos.Response
{
    public class UserInfoResponseDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Picture { get; set; }
        public DateTime DateCreate { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}