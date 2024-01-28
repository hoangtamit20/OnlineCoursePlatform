using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.UserDtos.Request;
using OnlineCoursePlatform.DTOs.UserDtos.Response;

namespace OnlineCoursePlatform.Services.UserServices.Interfaces
{
    public interface IUserService
    {
        Task<(int statusCode, BaseResponseWithData<AddUserRolesResponseDto> result)> AddUserRolesServiceAsync(
            AddUserRolesRequestDto addUserRolesRequestDto);

        Task<(int statusCode, BaseResponseWithData<RemoveUserRolesResponseDto>? result)> RemoveUserRolesServiceAsync(
            RemoveUserRolesRequestDto removeUserRolesRequestDto);
        
        Task<(int statusCode, BaseResponseWithData<UserBaseInfoResponseDto> result)> GetUserBaseInfoServiceAsync();
    }
}