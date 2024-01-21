using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Helpers.Emails.QuickEmailVerificationHelpers;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResetPasswordService> _logger;
        private readonly OnlineCoursePlatformDbContext _dbContext;


        public ResetPasswordService(
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<ResetPasswordService> logger,
            OnlineCoursePlatformDbContext dbContext)
        => (_userManager, _emailSender, _configuration, _logger, _dbContext)
        = (userManager, emailSender, configuration, logger, dbContext);

        public async Task<(int statusCode, BaseResponseWithData<ResetPasswordResponseDto> data)> ResetPasswordServiceAsync
            (ResetPasswordRequestDto resetPasswordRequestDto)
        {
            // If data not valid

            // If user not exist by userid
            var userExist = await _userManager.FindByIdAsync(resetPasswordRequestDto.Id);
            if (userExist is null)
            {
                _logger.LogWarning($"Cannot not found any user exists with id '{resetPasswordRequestDto.Id}'");
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot not found any user exists with id '{resetPasswordRequestDto.Id}'",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Reset password falied",
                    null
                ); 
            }
            // If confirm reset password fail
            var resetPasswordResult = await CheckResetPasswordAsync(
                user: userExist, 
                tokenResetPassword: resetPasswordRequestDto.Token, 
                newPassword: resetPasswordRequestDto.NewPassword);
            if (!resetPasswordResult.success)
            {
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: resetPasswordResult.message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Reset password falied",
                    null
                ); 
            }
            // Change password for user
            return BaseReturnHelper<ResetPasswordResponseDto>.GenerateSuccessResponse(
                data: new ResetPasswordResponseDto(){Message = "Congratulations! You have successfully changed your password."},
                message: "Reset password successed"
            );
        }


        public async Task<(int statusCode, BaseResponseWithData<ResetPasswordResponseDto> data)> CheckEmailResetPasswordServiceAsync(
            CheckEmailResetPasswordDto checkEmailResetPasswordDto
        )
        {
            // validation data

            // If email not exists on any services providers
            if (await CheckEmailExistOnServiceProviderAsync(checkEmailResetPasswordDto.Email))
            {
                _logger.LogWarning("The email address does not exist on any email service provider's system");
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: "The email address does not exist on any email service provider's system",
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Reset password falied",
                    null
                );
            }

            // If email not exists on system
            var userExist = await CheckUserExistsAsync(checkEmailResetPasswordDto.Email);
            if (userExist is null)
            {
                _logger.LogWarning($"The email address '{checkEmailResetPasswordDto.Email}' does not exist");
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: $"The email address '{checkEmailResetPasswordDto.Email}' does not exist",
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Reset password falied",
                    null
                );
            }
            // Get Url to navigate reset password page
            var existUrlResult = await GetUrlNavigateToRestPasswordPage();
            // If url not exists
            if (!existUrlResult.success && existUrlResult.url is null)
            {
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: existUrlResult.message ?? string.Empty,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Internal Server Error",
                    null
                );
            }
            // Process send email
            // If send email fail
            if (!await SendEmailResetPasswordConfirmationAsync(
                user: userExist, 
                urlPageRestPassword: existUrlResult.url!))
            {
                return BaseReturnHelper<ResetPasswordResponseDto>.GenerateErrorResponse(
                    errorMessage: "Failed to send confirmation email",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "InternalServerError : Failed to send confirmation email",
                    null
                );
            }
            // Send email confirm success
            _logger.LogInformation($"Send email address {checkEmailResetPasswordDto.Email} sucessed.");
            return BaseReturnHelper<ResetPasswordResponseDto>.GenerateSuccessResponse(
                data: new ResetPasswordResponseDto(){Message = "We have sent an email to your registered address for confirmation. Please check your email and follow the instructions to reset your password."},
                message: "Send email success."
            );
        }


        private async Task<AppUser?> CheckUserExistsAsync(string email)
        => await _userManager.FindByEmailAsync(email);

        private async Task<bool> CheckEmailExistOnServiceProviderAsync(string email)
        {
            EmailVerificationModel? emailVerificationModel = await QuickEmailVerificationHelper.ValidateEmailAddressAsync(
                emailAddress: email,
                baseUrl: _configuration.GetSection("QuickEmailValidHelper:BaseUrl").Value!,
                apiKey: _configuration.GetSection("QuickEmailValidHelper:ApiKey").Value!);
            return (emailVerificationModel is null
                || _configuration.GetSection("QuickEmailValidHelper:ErrorResult").Value!
                    == emailVerificationModel.result);
        }

        private async Task<(string? url, bool success, string? message)> GetUrlNavigateToRestPasswordPage()
        {
            var urlHelper = await _dbContext.UrlHelperEntities.FirstOrDefaultAsync();
            if (urlHelper is null || string.IsNullOrEmpty(urlHelper.ResetPasswordUrl))
            {
                _logger.LogWarning($"The Url navigate to Reset Password page is not found.");
                return (url: null, success: false, message: "The Url navigate to Reset Password page is not found.");
            }
            return (url: urlHelper.ResetPasswordUrl, success: true, message: null);
        }

        private async Task<bool> SendEmailResetPasswordConfirmationAsync(AppUser user, string urlPageRestPassword)
        {
            try
            {
                var confirmResetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                Uri finalUrl = BaseHelper.CreateConfirmationUrl(
                    baseUrl: urlPageRestPassword, userId: user.Id.ToString(), token: confirmResetPasswordToken);
                _logger.LogInformation($"Generated confirmation URL: {finalUrl}");

                await BaseHelper.SendConfirmationEmailAsync(_emailSender: _emailSender, email: user.Email!, finalUrl);
                _logger.LogInformation($"Sent confirmation email to {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Errortrace : {ex.Message}");
                return false;
            }
        }

        private async Task<(bool success, string message)> CheckResetPasswordAsync(
            AppUser user, string tokenResetPassword, string newPassword)
        {
            var resetPasswordResult = await _userManager.ResetPasswordAsync(
                user: user, token: tokenResetPassword, newPassword: newPassword);
            if (!resetPasswordResult.Succeeded)
            {
                var errorMessage = resetPasswordResult.Errors.Select(e => e.Description).ToList().ToString();
                _logger.LogWarning(errorMessage);
                return (success: false, message: errorMessage ?? string.Empty);
            }
            _logger.LogInformation($"Reset password for user {user.Email} successed");
            return (success: true, message: "Reset password success");
        }
    }
}