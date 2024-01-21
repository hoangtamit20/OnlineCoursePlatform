using OnlineCoursePlatform.Base;

namespace OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos
{
    public class ResetPasswordRequestDto : BaseRequestQueryDto
    {
        public string NewPassword { get; set; } = null!;
    }
}