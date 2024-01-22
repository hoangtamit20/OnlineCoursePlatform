using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface ILogOutService
    {
        Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutCurrentDeviceServiceAsync();
        Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutAllDeviceServiceAsync();
    }
}