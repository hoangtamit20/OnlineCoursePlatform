using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface IRoleService
    {
        Task<RoleResultDto> UpdateRoleServiceAsync(AppUser user, string oldRoleName, string newRoleName);
        Task<RoleResultDto> RemoveRoleServiceAsync(AppUser user, string roleName);
        Task<RoleResultDto> AddRoleServiceAsync(AppUser user, string roleName);
    }
}