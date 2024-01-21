using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.AuthModels;

namespace OnlineCoursePlatform.Services.AuthServices.IAuthServices
{
    public interface IJwtService
    {
        Task<TokenModel> GenerateJwtTokenAsync(AppUser user, string? ipAddress);
    }
}