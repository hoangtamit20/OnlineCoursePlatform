using Microsoft.AspNetCore.Identity;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleService> _logger;
        private readonly IConfiguration _configuration;
        public RoleService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleService> logger,
            IConfiguration configuration)
        => (_userManager, _roleManager, _logger, _configuration) 
        = (userManager, roleManager, logger, configuration);

        // Adds a role to a user if the user doesn't have it
        public async Task<RoleResultDto> AddRoleServiceAsync(AppUser user, string roleName)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role {roleName} added to user {user.UserName}.");
                    return new RoleResultDto(){ IsSuccess = true, Message = $"Role {roleName} added successfully."};
                }
                else
                {
                    _logger.LogError($"Failed to add role {roleName} to user {user.UserName}.");
                    return new RoleResultDto(){ IsSuccess = false, Message = $"Failed to add role {roleName}."};
                }
            }
            else
            {
                _logger.LogInformation($"User {user.UserName} already has the role {roleName}.");
                return new RoleResultDto(){ IsSuccess = false, Message = $"User already has the role {roleName}."};
            }
        }

        // Removes a role from a user if the user has it
        public async Task<RoleResultDto> RemoveRoleServiceAsync(AppUser user, string roleName)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(roleName))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role {roleName} removed from user {user.UserName}.");
                    return new RoleResultDto(){ IsSuccess = true, Message = $"Role {roleName} removed successfully."};
                }
                else
                {
                    _logger.LogError($"Failed to remove role {roleName} from user {user.UserName}.");
                    return new RoleResultDto(){ IsSuccess = false, Message = $"Failed to remove role {roleName}."};
                }
            }
            else
            {
                _logger.LogInformation($"User {user.UserName} does not have the role {roleName}.");
                return new RoleResultDto(){ IsSuccess = false, Message = $"User does not have the role {roleName}."};
            }
        }

        // Updates a user's role from oldRoleName to newRoleName
        public async Task<RoleResultDto> UpdateRoleServiceAsync(AppUser user, string oldRoleName, string newRoleName)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(oldRoleName))
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, oldRoleName);
                if (removeResult.Succeeded)
                {
                    var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
                    if (addResult.Succeeded)
                    {
                        _logger.LogInformation($"Role for user {user.UserName} updated from {oldRoleName} to {newRoleName}.");
                        return new RoleResultDto(){ IsSuccess = true, Message = $"Role updated from {oldRoleName} to {newRoleName} successfully."};
                    }
                    else
                    {
                        _logger.LogError($"Failed to add new role {newRoleName} to user {user.UserName}.");
                        return new RoleResultDto(){ IsSuccess = false, Message = $"Failed to update role to {newRoleName}."};
                    }
                }
                else
                {
                    _logger.LogError($"Failed to remove old role {oldRoleName} from user {user.UserName}.");
                    return new RoleResultDto(){ IsSuccess = false, Message = $"Failed to update role from {oldRoleName}."};
                }
            }
            else
            {
                _logger.LogInformation($"User {user.UserName} does not have the role {oldRoleName}.");
                return new RoleResultDto(){ IsSuccess = false, Message = $"User does not have the role {oldRoleName}."};
            }
        }
    }
}