using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.AuthDtos
{
    public class CheckEmailResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;
    }
}