using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface IRegisterService
    {
        Task<(int, BaseResponseWithData<RegisterResponseDto>)> RegisterServiceAsync(RegisterRequestDto registerRequestDto);
    }
}