using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.AuthDtos
{
    public class RegisterRequestDto : BaseAuthDto
    {
        [Required(ErrorMessage = "{0} is required.")]
        [StringLength(
            maximumLength : 50,
            MinimumLength = 5,
            ErrorMessage = "{0} must be between {2} and {1} characters long.")]
        public string Name { get; set; } = null!;
    }
}
