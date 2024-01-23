using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface IRegisterService
    {
        Task<(int, BaseResponseWithData<RegisterResponseDto>)> RegisterServiceAsync(RegisterRequestDto registerRequestDto);
        Task<(int statusCode, BaseResponseWithData<GoogleLoginResponseDto> result)> LoginWithGoogleServiceAsync(
            GoogleLoginRequestDto googleLoginRequestDto, string? ipAddress);
    }
}