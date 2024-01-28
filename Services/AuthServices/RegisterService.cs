using Google.Apis.Auth;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Helpers.Emails.QuickEmailVerificationHelpers;
using OnlineCoursePlatform.Models.Google;
using OnlineCoursePlatform.Repositories.AuthRepositories;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRepository> _logger;

        public RegisterService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            OnlineCoursePlatformDbContext dbContext,
            IJwtService jwtService,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<AuthRepository> logger)
        => (_userManager, _roleManager, _dbContext, _jwtService, _emailSender, _configuration, _logger)
        = (userManager, roleManager, dbContext, jwtService, emailSender, configuration, logger);



        public async Task<(int, BaseResponseWithData<RegisterResponseDto>)> RegisterServiceAsync(
            RegisterRequestDto registerRequestDto)
        {
            _logger.LogInformation("Starting user registration process");

            if (!await IsEmailValid(registerRequestDto))
            {
                return GenerateErrorResponse(
                    "The email address does not exist on any email service provider's system",
                    StatusCodes.Status400BadRequest);
            }

            if (await IsEmailAlreadyRegistered(registerRequestDto.Email))
            {
                return GenerateErrorResponse(
                    "Email already exists on the system",
                    StatusCodes.Status409Conflict);
            }

            var (isSuccessCreatUser, user) = await CreateUserAsync(registerRequestDto);
            if (!isSuccessCreatUser.Succeeded)
            {
                return GenerateErrorResponse(
                    "An internal server error occurred while creating the user",
                    StatusCodes.Status500InternalServerError);
            }

            if (!await AddUserToRole(registerRequestDto.Email))
            {
                return GenerateErrorResponse(
                    "Failed to add user to role",
                    StatusCodes.Status500InternalServerError);
            }

            if (!await SendConfirmationEmail(user))
            {
                return GenerateErrorResponse(
                    "Failed to send confirmation email",
                    StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("User registration process completed successfully");
            return GenerateSuccessResponse("Please check your email to confirm your account.");
        }

        private async Task<bool> IsEmailValid(RegisterRequestDto registerRequestDto)
        {
            if (!await CheckEmailExistOnServiceProviderAsync(registerRequestDto: registerRequestDto))
            {
                _logger.LogError("The email address does not exist on any email service provider's system");
                return false;
            }
            return true;
        }

        private async Task<bool> IsEmailAlreadyRegistered(string email)
        {
            var userExist = await CheckUserExistsAsync(email);
            if (userExist is not null)
            {
                _logger.LogError("Email already exists on the system");
                return true;
            }
            return false;
        }

        private async Task<bool> AddUserToRole(string email)
        {
            var roleResultDto = await AddUserToRoleAsync(email);
            if (!roleResultDto.IsSuccess)
            {
                _logger.LogError($"Failed to add user to role: {roleResultDto.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> SendConfirmationEmail(AppUser user)
        {
            var urlConfirmEmail = await GetConfirmationUrl();
            if (string.IsNullOrEmpty(urlConfirmEmail))
            {
                _logger.LogError("URL for email confirmation is not set");
                return false;
            }

            try
            {
                var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                Uri finalUrl = BaseHelper.CreateConfirmationUrl(urlConfirmEmail, user.Id.ToString(), confirmEmailToken);
                _logger.LogInformation($"Generated confirmation URL: {finalUrl}");

                await BaseHelper.SendConfirmationEmailAsync(_emailSender: _emailSender, email: user.Email!, finalUrl);
                _logger.LogInformation($"Sent confirmation email to {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errortrace : {ex.Message}");
                return false;
            }

        }

        private (int, BaseResponseWithData<RegisterResponseDto>) GenerateErrorResponse(string errorMessage, int statusCode)
        {
            return BaseReturnHelper<RegisterResponseDto>.GenerateErrorResponse(
                errorMessage: errorMessage,
                statusCode: statusCode,
                message: "Registration failed",
                data: null);
        }

        private (int, BaseResponseWithData<RegisterResponseDto>) GenerateSuccessResponse(string successMessage)
        {
            return BaseReturnHelper<RegisterResponseDto>.GenerateSuccessResponse(
                data: new RegisterResponseDto() { Message = successMessage },
                message: "Email was sent successfully.");
        }



        private async Task<bool> CheckEmailExistOnServiceProviderAsync(RegisterRequestDto registerRequestDto)
        {
            EmailVerificationModel? emailVerificationModel = await QuickEmailVerificationHelper.ValidateEmailAddressAsync(
                emailAddress: registerRequestDto.Email,
                baseUrl: _configuration.GetSection("QuickEmailValidHelper:BaseUrl").Value!,
                apiKey: _configuration.GetSection("QuickEmailValidHelper:ApiKey").Value!);
            if (emailVerificationModel is null
                || _configuration.GetSection("QuickEmailValidHelper:ErrorResult").Value!
                    == emailVerificationModel.result)
            {
                return false;
            }
            return true;
        }

        private async Task<AppUser?> CheckUserExistsAsync(string email)
        => await _userManager.FindByEmailAsync(email);

        private async Task<(IdentityResult, AppUser)> CreateUserAsync(RegisterRequestDto registerRequestDto)
        {
            var newUser = new AppUser()
            {
                Email = registerRequestDto.Email,
                UserName = registerRequestDto.Email,
                Name = registerRequestDto.Name
            };

            return (await _userManager.CreateAsync(newUser, registerRequestDto.Password), newUser);
        }



        public async Task<(int statusCode, BaseResponseWithData<GoogleLoginResponseDto> result)> LoginWithGoogleServiceAsync(
            GoogleLoginRequestDto googleLoginRequestDto, string? ipAddress)
        {
            // If data is valid

            // If get data success from access token of google
            try
            {
                var userInfoFromAccessTokenGoogle = await GetUserInfoFromGoogleTokenAsync(
                    googleToken: googleLoginRequestDto.IdToken);
                // If email is not exists on system
                var userExists = await _userManager.FindByEmailAsync(userInfoFromAccessTokenGoogle.Email);
                if (userExists is null)
                {
                    userExists = new AppUser()
                    {
                        Email = userInfoFromAccessTokenGoogle.Email,
                        Name = userInfoFromAccessTokenGoogle.Name,
                        Picture = userInfoFromAccessTokenGoogle.Picture,
                        UserName = userInfoFromAccessTokenGoogle.Email,
                        EmailConfirmed = true
                    };
                    // If create user success
                    if ((await _userManager.CreateAsync(user: userExists)).Succeeded)
                    {
                        // If add role for user failed
                        if (!await AddUserToRole(userExists.Email))
                        {
                            _logger.LogError("An internal server error occurred while add role for user");
                            return BaseReturnHelper<GoogleLoginResponseDto>.GenerateErrorResponse(
                                errorMessage: "An internal server error occurred while add role for user",
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Login with google failed",
                                data: null);
                        }
                        // Generate token
                        var tokenModel = await _jwtService.GenerateJwtTokenAsync(user: userExists, ipAddress: ipAddress);
                        return BaseReturnHelper<GoogleLoginResponseDto>.GenerateSuccessResponse(
                            data: tokenModel.Adapt<GoogleLoginResponseDto>(),
                            message: "Login with google success"
                        );
                    }
                    _logger.LogError("An internal server error occurred while creating the user");
                    return BaseReturnHelper<GoogleLoginResponseDto>.GenerateErrorResponse(
                        errorMessage: "An internal server error occurred while creating the user",
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Login with google failed",
                        data: null);
                }
                // If user is exists
                // Generate token
                var tokenUserExistsModel = await _jwtService.GenerateJwtTokenAsync(user: userExists, ipAddress: ipAddress);
                return BaseReturnHelper<GoogleLoginResponseDto>.GenerateSuccessResponse(
                    data: tokenUserExistsModel.Adapt<GoogleLoginResponseDto>(),
                    message: "Login with google success"
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BaseReturnHelper<GoogleLoginResponseDto>.GenerateErrorResponse(
                        errorMessage: ex.Message,
                        statusCode: StatusCodes.Status401Unauthorized,
                        message: "Login with google failed",
                        data: null
                    );
            }
        }



        public async Task<UserInfoFromIdTokenGoogle> GetUserInfoFromGoogleTokenAsync(string googleToken)
        {
            var clientId = _configuration["Google:ClientId"]!;
            var clientIdMobile = _configuration["Google:ClientIdMobile"]!;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { clientId, clientIdMobile},
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                return payload.Adapt<UserInfoFromIdTokenGoogle>();
            }
            catch (Exception ex)
            {
                // Try to get user info from access token if ID token fails
                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync($"{GoogleApiUrlConstant.UrlTokenInfo}{googleToken}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var tokenInfo = JsonConvert.DeserializeObject<TokenInfoRequestDto>(content);

                        if (
                            tokenInfo?.audience == clientId 
                            || tokenInfo?.audience == clientIdMobile)
                        {
                            HttpResponseMessage userInfoResponse = await client.GetAsync($"{GoogleApiUrlConstant.UrlUserInfo}{googleToken}");

                            if (userInfoResponse.IsSuccessStatusCode)
                            {
                                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                                var userInfo = JsonConvert.DeserializeObject<GoogleUserInfoRequestDto>(userInfoContent);
                                var user = new UserInfoFromIdTokenGoogle()
                                {
                                    Email = userInfo?.email!,
                                    Name = userInfo?.name!,
                                    Picture = userInfo?.picture
                                };
                                return user;
                            }
                        }
                    }

                    _logger.LogError($"Internal Server Error : Failed to verify Google Access token.\nTrace Log : {ex.Message}");
                    throw new Exception("Failed to verify Google Access token", ex);
                }
                // If idtoken and accesstoken was failed
                catch (Exception ex1)
                {
                    _logger.LogError($"Internal Server Error : Failed to get user info from Google Access token.\nTrace Log : {ex.Message}");
                    throw new Exception("Failed to get user info from Google Access token", ex1);
                }
            }
        }


        private async Task<RoleResultDto> AddUserToRoleAsync(string email)
        {
            var result = new RoleResultDto();
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                if (!await _roleManager.RoleExistsAsync(RolesConstant.Learner))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(RolesConstant.Learner));
                    if (!roleResult.Succeeded)
                    {
                        // For example, log the error or throw an exception
                        _logger.LogError("Error when creating role");
                        result.IsSuccess = false;
                        result.Message = "Error when creating role";
                        return result;
                    }
                }
                var addToRoleResult = await _userManager.AddToRoleAsync(user, RolesConstant.Learner);
                if (!addToRoleResult.Succeeded)
                {
                    // For example, log the error or throw an exception
                    _logger.LogError("Error when adding user to role");
                    result.IsSuccess = false;
                    result.Message = "Error when adding user to role";
                    return result;
                }
            }
            result.IsSuccess = true;
            result.Message = "User added to role successfully";
            return result;
        }

        private async Task<string?> GetConfirmationUrl()
        {
            return (await _dbContext.UrlHelperEntities.FirstOrDefaultAsync())?.ConfirmEmailFromClientUrl;
            // return @"https://localhost:7209/api/v1/auth/confirm-email";
        }
    }
}