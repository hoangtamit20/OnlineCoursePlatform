using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;

namespace OnlineCoursePlatform.Repositories.AuthRepositories
{
    public interface IAuthRepository
    {
        Task<(int, BaseResponseWithData<RegisterResponseDto>)> RegisterRepositoryAsync(RegisterRequestDto registerRequestDto);
        Task<(int, BaseResponseWithData<LoginResponseDto>)> LoginRepositoryAsync(LoginRequestDto loginRequestDto, string? ipAddress);
        Task<(int, BaseResponseWithData<ConfirmEmailResponseDto>)> ConfirmEmailRepositoryAsync(string id, string token);
        Task<(int, BaseResponseWithData<ResetPasswordResponseDto>)> 
            CheckEmailResetPasswordRepositoryAsync(CheckEmailResetPasswordDto checkEmailResetPasswordDto);
        Task<(int, BaseResponseWithData<ResetPasswordResponseDto>)> ResetPasswordRepositoryAsync(
            ResetPasswordRequestDto resetPasswordRequestDto);
        Task<BaseResponseWithData<LoginResponseDto>> LoginWithGoogle(string idToken);
        // Task<bool> UserExists(string username);
        // Task<bool> ResetPassword(string username, string newPassword);
    }
}