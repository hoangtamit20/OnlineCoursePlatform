using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OnlineCoursePlatform.Repositories.RoleRepositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<IdentityRole?> GetRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<IdentityResult> CreateRoleAsync(IdentityRole role)
        {
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
        {
            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(IdentityRole role)
        {
            return await _roleManager.DeleteAsync(role);
        }
    }
}