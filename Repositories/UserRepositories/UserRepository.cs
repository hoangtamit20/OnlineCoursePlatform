using Microsoft.AspNetCore.Identity;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Repositories.UserRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<AppUser?> FindUserByIdAsync(string userId)
            => await _userManager.FindByIdAsync(userId: userId);

        public async Task<IdentityResult> AddRolesForUserAsync(AppUser user, List<string> roleNames)
        {
            // Check if all roles exist
            var checkResult = await CheckRolesExistsAsync(roleNames);
            // If all roles exist, add them to the user, otherwise return the check result
            return checkResult.Succeeded ? await _userManager.AddToRolesAsync(user, roleNames) : checkResult;
        }

        public async Task<IdentityResult> RemoveRolesForUserAsync(AppUser user, List<string> roleNames)
        {
            // Check if all roles exist
            var checkResult = await CheckRolesExistsAsync(roleNames);
            // If all roles exist, remove them from the user, otherwise return the check result
            return checkResult.Succeeded ? await _userManager.RemoveFromRolesAsync(user, roleNames) : checkResult;
        }

        private async Task<IdentityResult> CheckRolesExistsAsync(List<string> roleNames)
        {
            // Iterate over each role in roleNames
            foreach (var roleName in roleNames)
            {
                // Check if the role exists
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    // If a role does not exist, return IdentityResult with an error
                    return IdentityResult.Failed(
                        new IdentityError {
                            Code = StatusCodes.Status404NotFound.ToString(),
                            Description = $"Role '{roleName}' does not exist." });
                }
            }
            // If all roles exist, return a successful IdentityResult
            return IdentityResult.Success;
        }

        public async Task<AppUser?> GetUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId: userId);
    }
}