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
using OnlineCoursePlatform.Helpers.UrlHelpers;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Repositories.AuthRepositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IEmailSender _emailSender;
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILoginService _loginService;
        private readonly IRegisterService _registerService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly ILogOutService _logOutService;

        public AuthRepository(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            ILogger<AuthRepository> logger,
            IEmailSender emailSender,
            OnlineCoursePlatformDbContext dbContext,
            ILoginService loginService,
            IRegisterService registerService,
            IResetPasswordService resetPasswordService,
            ILogOutService logOutService)
        => (_userManager, _configuration, _logger, _emailSender, _dbContext, _loginService, _registerService, _resetPasswordService, _logOutService)
        = (userManager, configuration, logger, emailSender, dbContext, loginService, registerService, resetPasswordService, logOutService);

        public async Task<(int, BaseResponseWithData<RegisterResponseDto>)> RegisterRepositoryAsync(
            RegisterRequestDto registerRequestDto)
            => await _registerService.RegisterServiceAsync(
                registerRequestDto: registerRequestDto);

        public async Task<(int, BaseResponseWithData<LoginResponseDto>)> LoginRepositoryAsync(
            LoginRequestDto loginRequestDto,
            string? ipAddress)
            => await _loginService.LoginServiceAsync(loginRequestDto, ipAddress);


        
        public async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutCurrentDeviceRepositoryAsync()
        => await _logOutService.LogOutCurrentDeviceServiceAsync();

        public async Task<(int statusCode, BaseResponseWithData<LogOutResponseDto> result)> LogOutAllDeviceRepositoryAsync()
        => await _logOutService.LogOutAllDeviceServiceAsync();


        public async Task<(int statusCode, BaseResponseWithData<RefreshTokenResponseDto> result)> RefreshTokenRepositoryAsync(
            RefreshTokenRequestDto refreshTokenRequestDto)
        => await _loginService.RefreshTokenServiceAsync(refreshTokenRequestDto: refreshTokenRequestDto);





        public async Task<(int, BaseResponseWithData<ConfirmEmailResponseDto>)> ConfirmEmailRepositoryAsync(string id, string token)
        {
            UrlHelperEntity? urlHelper = await _dbContext.UrlHelperEntities.FirstOrDefaultAsync();
            // If data is not valid
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Both id and token are required.");
                return BaseReturnHelper<ConfirmEmailResponseDto>.GenerateErrorResponse(
                    errorMessage: "Both id and token are required.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Confirmed email falied",
                    data: new ConfirmEmailResponseDto(){
                        ConfirmEmailUrl = urlHelper!.ConfirmEmailUrl}
                );
            }
            // If process of decode token from url fail
            var resultDecodeToken = BaseHelper.DecodeTokenFromUrl(tokenFromUrl: token);
            if (!resultDecodeToken.IsSuccess)
            {
                _logger.LogError(resultDecodeToken.Message);
                return BaseReturnHelper<ConfirmEmailResponseDto>.GenerateErrorResponse(
                    errorMessage: resultDecodeToken.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Internal Server Error",
                    data: new ConfirmEmailResponseDto(){
                        ConfirmEmailUrl = urlHelper!.ConfirmEmailUrl}
                );
            }
            var userExists = await _userManager.FindByIdAsync(id);
            // If user not exists
            if (userExists is null)
            {
                _logger.LogWarning($"Cannot found user of id : '{id}'");
                return BaseReturnHelper<ConfirmEmailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found user of id : '{id}'",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Confirm email failed",
                    data: new ConfirmEmailResponseDto(){
                        ConfirmEmailUrl = urlHelper!.ConfirmEmailUrl}
                );
            }
            // If confirm email fail
            var confirmEmailResult = await _userManager.ConfirmEmailAsync(user: userExists, token: token);
            if (!confirmEmailResult.Succeeded)
            {
                var errorMessageConfirmEmail = confirmEmailResult.Errors.Select(e => e.Description).ToList().ToString();
                _logger.LogError(errorMessageConfirmEmail);
                return BaseReturnHelper<ConfirmEmailResponseDto>.GenerateErrorResponse(
                    errorMessage: errorMessageConfirmEmail ?? string.Empty,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Internal Server Error",
                    data: new ConfirmEmailResponseDto(){
                        ConfirmEmailUrl = urlHelper!.ConfirmEmailUrl}
                );
            }
            return BaseReturnHelper<ConfirmEmailResponseDto>
                .GenerateSuccessResponse(
                    data: new ConfirmEmailResponseDto(){
                        ConfirmEmailUrl = urlHelper!.ConfirmEmailUrl},
                    message: "Confirm email successed.");
        }

        public Task<BaseResponseWithData<LoginResponseDto>> LoginWithGoogle(string idToken)
        {
            throw new NotImplementedException();
        }


        /*----------------------- Reset password part ---------------------------------*/
        public async Task<(int, BaseResponseWithData<ResetPasswordResponseDto>)> 
            CheckEmailResetPasswordRepositoryAsync(CheckEmailResetPasswordDto checkEmailResetPasswordDto)
            => await _resetPasswordService.CheckEmailResetPasswordServiceAsync(checkEmailResetPasswordDto: checkEmailResetPasswordDto);

        public async Task<(int, BaseResponseWithData<ResetPasswordResponseDto>)> ResetPasswordRepositoryAsync(
            ResetPasswordRequestDto resetPasswordRequestDto)
            => await _resetPasswordService.ResetPasswordServiceAsync(resetPasswordRequestDto: resetPasswordRequestDto);
    }
}