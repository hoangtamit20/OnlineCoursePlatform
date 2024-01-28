using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.UserDtos.Request
{
    public class AddUserRolesRequestDto
    {
        public string UserId { get; set; } = null!;
        [Required(ErrorMessage = "{0} is require.")]
        public List<string> RoleNames { get; set; } = new();
    }
}