using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.RoleDtos.Request;
using OnlineCoursePlatform.DTOs.RoleDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Repositories.RoleRepositories;
using OnlineCoursePlatform.Services.RoleServices.Interfaces;

namespace OnlineCoursePlatform.Services.RoleServices.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleService> _logger;
        public RoleService(
            IRoleRepository roleRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

    }
}