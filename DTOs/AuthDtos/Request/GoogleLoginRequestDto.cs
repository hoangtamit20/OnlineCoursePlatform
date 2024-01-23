using OnlineCoursePlatform.Models.Attributes;

namespace OnlineCoursePlatform.DTOs.AuthDtos.Request
{
    public class GoogleLoginRequestDto
    {
        [ValidateGoogleIdToken]
        public string IdToken { get; set; } = null!;
    }
}