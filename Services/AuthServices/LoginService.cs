using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Repositories.AuthRepositories;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IEmailSender _emailSender;
        private readonly OnlineCoursePlatformDbContext _dbContext;

        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public LoginService(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            ILogger<AuthRepository> logger,
            IEmailSender emailSender,
            OnlineCoursePlatformDbContext dbContext,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        public async Task<(int, BaseResponseWithData<LoginResponseDto>)> LoginServiceAsync(
            LoginRequestDto loginRequestDto,
            string? ipAddress)
        {
            // Check if the user with the provided email exists.
            var userExists = await CheckUserExistsAsync(loginRequestDto.Email);

            // If the user does not exist, return an error response indicating that the email does not exist.
            if (userExists is null)
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message: "Login failed",
                    errorMessage: "Email does not exist",
                    statusCode: StatusCodes.Status404NotFound);

            // Validate the password provided by the user.
            var isPasswordCorrect = await ValidatePasswordAsync(userExists, loginRequestDto.Password);

            // If the password is incorrect, return an error response indicating that the password is incorrect.
            if (!isPasswordCorrect)
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message: "Login failed",
                    errorMessage: "Incorrect password",
                    statusCode: StatusCodes.Status401Unauthorized);

            // If email not yet confirmed
            var (isEmailConfirmed, messageError) = await CheckEmailConfirmedAsync(user: userExists);
            if (!isEmailConfirmed && messageError != null)
            {
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message: "Login failed",
                    errorMessage: messageError,
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            try
            {
                // Generate a JWT token for the user.
                var tokenModel = await _jwtService.GenerateJwtTokenAsync(userExists, ipAddress);
                // If the token is successfully generated, return a success response with the token.
                return BaseReturnHelper<LoginRequestDto>.GenerateSuccessResponse(tokenModel: tokenModel, message: "Login successed");
            }
            catch (DbUpdateException)
            {
                // If there is a database update exception while updating the user's refresh token, return an error response.
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message: "Login failed",
                    errorMessage: "An error occurred while updating the user refresh token",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                // If any other exception occurs, log the error and return an error response.
                _logger.LogError(ex, $"An error occurred while updating the user at {DateTime.UtcNow}.");
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message: "Login failed",
                    errorMessage: "An error occurred while updating the user refresh token",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }



        public async Task<(int statusCode, BaseResponseWithData<RefreshTokenResponseDto> result)> RefreshTokenServiceAsync(
            RefreshTokenRequestDto refreshTokenRequestDto)
        {
            // If userRefreshToken not exists
            var userRefreshToken = await _dbContext.UserRefreshTokens
                .FirstOrDefaultAsync(usr => usr.RefreshToken == refreshTokenRequestDto.RefreshToken);
            if (userRefreshToken is null)
            {
                return BaseReturnHelper<RefreshTokenResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found any user has the refresh token : {refreshTokenRequestDto.RefreshToken}",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Refresh token falied",
                    data: null
                );
            }

            // If refresh token was expired or was revoked
            if (!userRefreshToken.Active || userRefreshToken.IsRevoked)
            {
                return BaseReturnHelper<RefreshTokenResponseDto>.GenerateErrorResponse(
                    errorMessage: "Refresh token expired or revoked",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Refresh token faied",
                    data: null
                );
            }

            // Generate new access token
            var accessToken = await _jwtService.GenerateAccessTokenAsync(
                user: (await _userManager.FindByIdAsync(userRefreshToken.UserId))!);
            // Update new accesstoken from current refresh token
            userRefreshToken.AccessToken = accessToken;
            _dbContext.UserRefreshTokens.Update(userRefreshToken);
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"User {userRefreshToken.UserId} get refresh token success.");
                return BaseReturnHelper<RefreshTokenResponseDto>.GenerateSuccessResponse(
                    data: new RefreshTokenResponseDto() { AccessToken = accessToken },
                    message: "Refresh token successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error : An error occurred while updating accesstoken");
                return BaseReturnHelper<RefreshTokenResponseDto>.GenerateErrorResponse(
                    errorMessage: "An error occurred while updating accesstoken",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Refresh token error",
                    data: null
                );
            }

        }

        private async Task<AppUser?> CheckUserExistsAsync(string email)
        => await _userManager.FindByEmailAsync(email);

        private async Task<bool> ValidatePasswordAsync(AppUser user, string password)
        => await _userManager.CheckPasswordAsync(user, password);


        private async Task<(bool, string?)> CheckEmailConfirmedAsync(AppUser user)
        {
            // Check if the user's email has been confirmed
            if (!user.EmailConfirmed)
            {
                // If not, generate a token for email confirmation
                var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation($"Generated confirmation token for user {user.Id}");

                // Get the URL to redirect the user after they confirm their email
                var urlConfirmEmail = GetConfirmationUrl();


                // Create the email confirmation URL by adding the userId and token to the query string
                Uri finalUrl = BaseHelper.CreateConfirmationUrl(urlConfirmEmail, user.Id.ToString(), confirmEmailToken);
                _logger.LogInformation($"Generated confirmation URL: {finalUrl}");

                // Send the confirmation email to the user
                await BaseHelper.SendConfirmationEmailAsync(_emailSender: _emailSender, email: user.Email!, finalUrl);
                _logger.LogInformation($"Sent confirmation email to {user.Email}");

                return (false, $"Please check your email to confirm your email address!");
            }
            return (true, null);
        }

        private string GetConfirmationUrl()
        {
            return @"https://localhost:7209/api/v1/auth/confirm-email";
        }
    }
}