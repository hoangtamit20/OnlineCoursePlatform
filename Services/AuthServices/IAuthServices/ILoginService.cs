using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface ILoginService
    {
        Task<(int, BaseResponseWithData<LoginResponseDto>)> LoginServiceAsync(
            LoginRequestDto loginRequestDto,
            string? ipAddress);
        Task<(int statusCode, BaseResponseWithData<RefreshTokenResponseDto> result)> RefreshTokenServiceAsync(
            RefreshTokenRequestDto refreshTokenRequestDto);
    }
}