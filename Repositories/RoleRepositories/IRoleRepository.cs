using Microsoft.AspNetCore.Identity;

namespace OnlineCoursePlatform.Repositories.RoleRepositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<IdentityRole?> GetRoleByIdAsync(string roleId);
        Task<IdentityRole?> GetRoleByNameAsync(string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task<IdentityResult> CreateRoleAsync(IdentityRole role);
        Task<IdentityResult> UpdateRoleAsync(IdentityRole role);
        Task<IdentityResult> DeleteRoleAsync(IdentityRole role);
    }
}