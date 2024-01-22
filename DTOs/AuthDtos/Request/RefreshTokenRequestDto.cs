using OnlineCoursePlatform.Models.Attributes;

namespace OnlineCoursePlatform.DTOs.AuthDtos.Request
{
    public class RefreshTokenRequestDto
    {
        [Guid]
        public string RefreshToken { get; set; } = null!;
    }
}