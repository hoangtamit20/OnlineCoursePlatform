namespace OnlineCoursePlatform.DTOs.UserDtos.Request
{
    public class RemoveUserRolesRequestDto
    {
        public string UserId { get; set; } = null!; 
        public List<string> RoleNames { get; set; } = new();
    }
}