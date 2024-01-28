using Microsoft.AspNetCore.Identity;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Repositories.UserRepositories
{
    public interface IUserRepository
    {
        Task<AppUser?> FindUserByIdAsync(string userId);
        Task<IdentityResult> AddRolesForUserAsync(AppUser user, List<string> roleNames);
        Task<IdentityResult> RemoveRolesForUserAsync(AppUser user, List<string> roleNames);
        Task<AppUser?> GetUserAsync(string userId);
        // Task<IdentityResult> UpdateRolesForUserAsync(AppUser user, List<string> roleNames);
    }
}