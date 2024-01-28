using OnlineCoursePlatform.Models.Attributes;

namespace OnlineCoursePlatform.DTOs.AuthDtos.Request
{
    public class GoogleLoginRequestDto
    {
        [ValidationTokenGoogle]
        public string IdToken { get; set; } = null!;
    }
}