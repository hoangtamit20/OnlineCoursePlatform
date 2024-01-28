namespace OnlineCoursePlatform.DTOs.RoleDtos.Response
{
    public class AddRoleResponseDto
    {
        public string Email { get; set; } = null!;
        public List<string> RoleNames { get; set; } = new();
    }
}