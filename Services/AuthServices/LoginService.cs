using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AuthDtos;
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
                    message : "Login failed",
                    errorMessage: "Email does not exist",
                    statusCode: StatusCodes.Status404NotFound);

            // Validate the password provided by the user.
            var isPasswordCorrect = await ValidatePasswordAsync(userExists, loginRequestDto.Password);

            // If the password is incorrect, return an error response indicating that the password is incorrect.
            if (!isPasswordCorrect)
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message : "Login failed",
                    errorMessage: "Incorrect password",
                    statusCode: StatusCodes.Status401Unauthorized);

            // If email not yet confirmed
            var (isEmailConfirmed, messageError) = await CheckEmailConfirmedAsync(user : userExists);
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
                // var tokenModel = new TokenModel(){
                //     AccessToken = await GenerateAccessTokenAsync(user: userExists),
                //     RefreshToken = ""
                // };

                // If the token is successfully generated, return a success response with the token.
                return BaseReturnHelper<LoginRequestDto>.GenerateSuccessResponse(tokenModel: tokenModel, message: "Login successed");
            }
            catch (DbUpdateException)
            {
                // If there is a database update exception while updating the user's refresh token, return an error response.
                return BaseReturnHelper<LoginRequestDto>.GenerateErrorResponse(
                    message : "Login failed",
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


        private async Task<string> GenerateAccessTokenAsync(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:SecretKey").Value!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email!)
            };
            // Add a claim for each role the user has.
            foreach (var role in await _userManager.GetRolesAsync(user))
                userClaims.Add(new Claim(ClaimTypes.Role, role));

            System.Console.WriteLine(_configuration.GetSection("JwtConfig:ValidIssuer").Value! + _configuration.GetSection("JwtConfig:ValidAudience").Value);
            
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JwtConfig:ValidIssuer").Value,
                audience: _configuration.GetSection("JwtConfig:ValidAudience").Value,
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}