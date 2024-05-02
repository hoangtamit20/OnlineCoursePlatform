using Microsoft.AspNetCore.Identity;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.UserDtos.Response;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.User;

namespace OnlineCoursePlatform.Repositories.UserRepositories
{
    public interface IUserRepository
    {
        Task<PagedList<UserInfoResponseDto>> GetAllUsersAsync(
            UserFilterParams pagingAndFilterParams);

        Task<AppUser?> FindUserByIdAsync(string userId);
        Task<IdentityResult> AddRolesForUserAsync(AppUser user, List<string> roleNames);
        Task<IdentityResult> RemoveRolesForUserAsync(AppUser user, List<string> roleNames);
        Task<AppUser?> GetUserAsync(string userId);
        // Task<IdentityResult> UpdateRolesForUserAsync(AppUser user, List<string> roleNames);
    }
}