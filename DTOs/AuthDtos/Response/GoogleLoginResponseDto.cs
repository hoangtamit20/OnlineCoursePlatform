using OnlineCoursePlatform.Models.AuthModels;

namespace OnlineCoursePlatform.DTOs.AuthDtos.Response
{
    public class GoogleLoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}