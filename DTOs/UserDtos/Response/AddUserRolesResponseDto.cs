namespace OnlineCoursePlatform.DTOs.UserDtos.Response
{
    public class AddUserRolesResponseDto
    {
        public string Email { get; set; } = null!;
        public List<string> RoleNames { get; set; } = new();
    }
}