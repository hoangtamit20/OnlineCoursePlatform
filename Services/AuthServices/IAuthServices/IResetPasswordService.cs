using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface IResetPasswordService
    {
        Task<(int statusCode, BaseResponseWithData<ResetPasswordResponseDto> data)> CheckEmailResetPasswordServiceAsync(
            CheckEmailResetPasswordDto checkEmailResetPasswordDto);
        Task<(int statusCode, BaseResponseWithData<ResetPasswordResponseDto> data)> ResetPasswordServiceAsync
            (ResetPasswordRequestDto resetPasswordRequestDto);
        
    }
}