using System.Security.Claims;
using Mapster;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.UserDtos.Request;
using OnlineCoursePlatform.DTOs.UserDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.User;
using OnlineCoursePlatform.Repositories.UserRepositories;
using OnlineCoursePlatform.Services.UserServices.Interfaces;

namespace OnlineCoursePlatform.Services.UserServices.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<PagedList<UserInfoResponseDto>> 
            GetAllUsersServiceAsync(UserFilterParams pagingAndFilterParams)
        => await _userRepository.GetAllUsersAsync(pagingAndFilterParams: pagingAndFilterParams);

        public async Task<(int statusCode, BaseResponseWithData<UserBaseInfoResponseDto> result)> GetUserBaseInfoServiceAsync()
        {
            // if get userid failed
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return BaseReturnHelper<UserBaseInfoResponseDto>.GenerateErrorResponse(
                    errorMessage: "Unauthorize",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Unauthorize",
                    data: null
                );
            }
            // If user is exists
            var userExists = await _userRepository.GetUserAsync(userId: userId);
            if (userExists is null)
            {
                return BaseReturnHelper<UserBaseInfoResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found user with id : {userId}",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get user falied",
                    data: null
                );
            }
            return BaseReturnHelper<UserBaseInfoResponseDto>.GenerateSuccessResponse(
                data: userExists.Adapt<UserBaseInfoResponseDto>(),
                message: "Get user info success"
            );
        }

        private string? GetCurrentUserId()
            => _httpContextAccessor?.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

        
































        /*----------------------------- PROCESS ROLES FOR USER ------------------------------*/

        public async Task<(int statusCode, BaseResponseWithData<AddUserRolesResponseDto> result)> AddUserRolesServiceAsync(
            AddUserRolesRequestDto addUserRolesRequestDto)
        {
            // If data is valid

            // If user is exists
            var userExists = await _userRepository.FindUserByIdAsync(userId: addUserRolesRequestDto.UserId);
            if (userExists is null)
            {
                _logger.LogWarning($"User {addUserRolesRequestDto.UserId} not found");
                return BaseReturnHelper<AddUserRolesResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found user '{addUserRolesRequestDto.UserId}' while add role",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Add role failed",
                    data: null
                );
            }
            // Add role for user
            var addRolesResult = await _userRepository.AddRolesForUserAsync(
                user: userExists,
                roleNames: addUserRolesRequestDto.RoleNames
            );

            // If add role falied
            var errorAddRoleResult = BaseHelper.GetErrorsFromIdentityResult(identityResult: addRolesResult);
            if (errorAddRoleResult is not null)
            {
                _logger.LogError(errorAddRoleResult);
                return BaseReturnHelper<AddUserRolesResponseDto>.GenerateErrorResponse(
                    errorMessage: errorAddRoleResult,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    message: "Add role failed",
                    data: null
                );
            }
            // Add role successed
            _logger.LogInformation(message: $"Add role for user {addUserRolesRequestDto.UserId} success.");
            return BaseReturnHelper<AddUserRolesResponseDto>.GenerateSuccessResponse(
                data: new AddUserRolesResponseDto()
                {
                    Email = userExists.Email!,
                    RoleNames = addUserRolesRequestDto.RoleNames
                },
                message: $"Add role for user {userExists.Email} success"
            );
        }


        public async Task<(int statusCode, BaseResponseWithData<RemoveUserRolesResponseDto>? result)> RemoveUserRolesServiceAsync(
            RemoveUserRolesRequestDto removeUserRolesRequestDto)
        {
            // If user is not exists
            var userExists = await _userRepository.FindUserByIdAsync(userId: removeUserRolesRequestDto.UserId);
            if (userExists is null)
            {
                _logger.LogWarning($"User {removeUserRolesRequestDto.UserId} not found");
                return BaseReturnHelper<RemoveUserRolesResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found user '{removeUserRolesRequestDto.UserId}' when remove role",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Add role failed",
                    data: null
                );
            }
            // Remove role for user
            var removeUserRoleResult = await _userRepository.RemoveRolesForUserAsync(
                user: userExists, removeUserRolesRequestDto.RoleNames);
            // If remove role failed
            var errorRemoveUserRoleResult = BaseHelper.GetErrorsFromIdentityResult(identityResult: removeUserRoleResult);
            if (errorRemoveUserRoleResult is not null)
            {
                _logger.LogError($"Internal Server Erorr : {errorRemoveUserRoleResult}");
                return BaseReturnHelper<RemoveUserRolesResponseDto>.GenerateErrorResponse(
                    errorMessage: $"An error occurred when remove role for user '{userExists.Email}'. {errorRemoveUserRoleResult}",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Remove role failed",
                    data: null
                );
            }
            // Remove user role success
            _logger.LogInformation(message: $"Add role for user {userExists.Id} success.");
            return (statusCode: StatusCodes.Status204NoContent, null);
        }
    }
}