using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.AuthDtos
{
    public class BaseAuthDto
    {
        /// <summary>
        /// Địa chỉ email của người dùng.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mật khẩu của người dùng.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            maximumLength: 100, 
            ErrorMessage = "The {0} must be at least {2} characters long.", 
            MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(
            pattern: @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$", 
            ErrorMessage = "The password must have at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public string Password { get; set; } = null!;
    }
}