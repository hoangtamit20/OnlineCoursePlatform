using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface ILoginService
    {
        Task<(int, BaseResponseWithData<LoginResponseDto>)> LoginServiceAsync(
            LoginRequestDto loginRequestDto,
            string? ipAddress);
    }
}