using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.UserDtos.Request;
using OnlineCoursePlatform.DTOs.UserDtos.Response;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.User;

namespace OnlineCoursePlatform.Services.UserServices.Interfaces
{
    public interface IUserService
    {
        Task<(int statusCode, BaseResponseWithData<AddUserRolesResponseDto> result)> AddUserRolesServiceAsync(
            AddUserRolesRequestDto addUserRolesRequestDto);

        Task<(int statusCode, BaseResponseWithData<RemoveUserRolesResponseDto>? result)> RemoveUserRolesServiceAsync(
            RemoveUserRolesRequestDto removeUserRolesRequestDto);
        
        Task<(int statusCode, BaseResponseWithData<UserBaseInfoResponseDto> result)> GetUserBaseInfoServiceAsync();

        Task<PagedList<UserInfoResponseDto>> 
            GetAllUsersServiceAsync(UserFilterParams pagingAndFilterParams);
    }
}