using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class LogOutService : ILogOutService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;

        private readonly ILogger<LogOutService> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogOutService(
            OnlineCoursePlatformDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LogOutService> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutCurrentDeviceServiceAsync()
        {
            var accessToken = GetAccessTokenFromHeader();
            if (accessToken is null)
            {
                return GenerateLogoutResponse();
            }

            var userRefreshToken = await GetUserRefreshToken(accessToken);
            if (userRefreshToken is null)
            {
                return GenerateLogoutResponse();
            }

            return await UpdateCurrentUserRefreshToken(userRefreshToken);
        }

        public async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutAllDeviceServiceAsync()
        {
            var accessToken = GetAccessTokenFromHeader();
            if (accessToken is null)
            {
                return GenerateLogoutResponse();
            }

            var userRefreshToken = await GetUserRefreshToken(accessToken);
            if (userRefreshToken is null)
            {
                return GenerateLogoutResponse();
            }

            return await UpdateAllUserRefreshToken(
                await GetAllUserRefreshToken(userId: userRefreshToken.UserId));
        }

        private string? GetAccessTokenFromHeader()
        {
            var httpRequest = _httpContextAccessor?.HttpContext?.Request;
            if (httpRequest!.Headers.TryGetValue("Authorization", out var accessTokenValue) &&
                accessTokenValue.ToString().StartsWith("Bearer "))
            {
                return accessTokenValue.ToString().Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        private async Task<UserRefreshToken?> GetUserRefreshToken(string accessToken)
        {
            return await _dbContext.UserRefreshTokens
                .FirstOrDefaultAsync(usr => usr.AccessToken == accessToken && usr.UserId == GetCurrentUserId());
        }

        private async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> UpdateCurrentUserRefreshToken(
            UserRefreshToken userRefreshToken)
        {
            userRefreshToken.LastRevoked = DateTime.UtcNow;
            userRefreshToken.IsRevoked = true;
            string errorMessage = string.Empty;
            try
            {
                _dbContext.Entry<UserRefreshToken>(userRefreshToken).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"User {userRefreshToken.UserId} logout success.");
                return GenerateLogoutResponse();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                errorMessage = "Concurrency error while updating UserRefreshToken";
                _logger.LogError($"Concurrency error while updating UserRefreshToken: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                errorMessage = "Database error while updating UserRefreshToken";
                _logger.LogError($"Database error while updating UserRefreshToken: {ex.Message}");
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while updating UserRefreshToken";
                _logger.LogError($"An error occurred while updating UserRefreshToken: {ex.Message}");
            }
            return (statusCode: StatusCodes.Status500InternalServerError,
                new BaseResponseWithData<LogOutResponseDto>()
                {
                    IsSuccess = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>() { errorMessage },
                    Data = null
                });
        }

        private async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> UpdateAllUserRefreshToken(
            List<UserRefreshToken> userRefreshToken)
        {
            foreach (var usr in userRefreshToken)
            {
                usr.LastRevoked = DateTime.UtcNow;
                usr.IsRevoked = true;
            }
            string errorMessage = string.Empty;
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"User {userRefreshToken.FirstOrDefault()?.UserId} logout all devices success.");
                return GenerateLogoutResponse();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                errorMessage = "Concurrency error while updating UserRefreshToken";
                _logger.LogError($"Concurrency error while updating UserRefreshToken: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                errorMessage = "Database error while updating UserRefreshToken";
                _logger.LogError($"Database error while updating UserRefreshToken: {ex.Message}");
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while updating UserRefreshToken";
                _logger.LogError($"An error occurred while updating UserRefreshToken: {ex.Message}");
            }
            return (statusCode: StatusCodes.Status500InternalServerError,
                new BaseResponseWithData<LogOutResponseDto>()
                {
                    IsSuccess = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>() { errorMessage },
                    Data = null
                });
        }

        private async Task<List<UserRefreshToken>> GetAllUserRefreshToken(string userId)
        {
            return await _dbContext.UserRefreshTokens
                .Where(usr => usr.UserId == userId)
                .ToListAsync();
        }

        private string? GetCurrentUserId()
            => _httpContextAccessor?.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

        private (int statusCode, BaseResponseWithData<LogOutResponseDto> result) GenerateLogoutResponse()
        {
            return (statusCode: StatusCodes.Status200OK,
                result: new BaseResponseWithData<LogOutResponseDto>()
                {
                    IsSuccess = true,
                    Message = "Logout success",
                    Data = null
                });
        }
    }
}