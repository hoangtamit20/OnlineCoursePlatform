using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.DTOs.AuthDtos.Response;
using OnlineCoursePlatform.Helpers.UrlHelpers;
using OnlineCoursePlatform.Repositories.AuthRepositories;
using OnlineCoursePlatform.Services.AuthServices.AuthServiceDtos;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthRepository> _logger;

        public AuthController(
            IAuthRepository authRepository,
            ILogger<AuthRepository> logger,
            IHttpContextAccessor httpContextAccessor
        )
        => (_authRepository, _logger)
        = (authRepository, logger);


        /// <summary>
        /// This API handles the user's login process.
        /// </summary>
        /// <param name="loginRequestDto">The username of the user.</param>
        /// <returns>Returns access token and refresh token if login is successful, otherwise returns an error message.</returns>
        /// <response code="200">Returns the tokens if login is successful</response>
        /// <response code="400">If the request is malformed or the content is not valid</response>
        /// <response code="401">If the provided credentials are incorrect or if the email is not confirmed.
        /// The response body will contain a more specific error message.</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="500">If there is a server error</response>
        /// <response code="503">If there is a server unavailable service</response>
        /// /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/auth/login
        ///     {
        ///         "email": "hoangtamit20@gmail.com",
        ///         "password": "Khuong@090217"
        ///     }
        /// 
        /// </remarks>
        [HttpPost("/api/v1/auth/login")]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(BaseResponseWithData<LoginResponseDto>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            // check data is valid
            if (ModelState.IsValid)
            {
                // Log the start of the login process for a user with a specific email at the current time.
                _logger.LogInformation($"Login started for user: {loginRequestDto.Email} at {DateTime.UtcNow}.");
                var (statusCode, data) = await _authRepository
                    .LoginRepositoryAsync(
                        loginRequestDto,
                        HttpContext.Connection.RemoteIpAddress?.ToString()
                    );
                // Log the successful completion of the login process for a user with a specific email at the current time.
                if (statusCode == 200)
                    _logger.LogInformation($"Login successful for user: {loginRequestDto.Email} at {DateTime.UtcNow}.");
                return StatusCode(statusCode, data);
            }
            return BadRequest(new BaseResponseWithData<LoginResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }


        /// <summary>
        /// This API handle the user's login with google process
        /// </summary>
        /// <param name="googleLoginRequestDto">The access token or id token when user login with google</param>
        /// <returns>Returns access token and refresh token if login is successful, otherwise returns an error message</returns>
        /// <response code="200">Returns the tokens if login is successful</response>
        /// <response code="400">If the request is malformed or the content is not valid</response>
        /// <response code="401">If the user is not found</response>
        /// <response code="500">If there is a server error</response>
        /// /// <remarks>
        /// Example:
        /// 
        ///     Requirements:
        ///     
        ///     ClientId: '942845050866-3mhpjrmr2icagmdeu1f9011og01q11da.apps.googleusercontent.com'
        ///     
        ///     ClientMobile : '151128183564-cdsuumfna586g715uiju2a4dqeo5i8jr.apps.googleusercontent.com'
        ///     
        ///     This is the token of your registered Google application. You will need it to validate the IdToken or AccessToken.
        ///    
        ///     IdToken: This is the token you receive from the client after the user successfully logs in with Google.
        ///     
        ///     POST /api/v1/auth/login-with-google
        ///     {
        ///        "IdToken": "{idtoken/accesstoken}",
        ///     }
        /// 
        /// </remarks>
        [HttpPost("/api/v1/auth/login-with-google")]
        [ProducesResponseType(type: typeof(BaseResponseWithData<LoginResponseDto>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(type: typeof(BaseResponseWithData<LoginResponseDto>), statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(type: typeof(BaseResponseWithData<LoginResponseDto>), statusCode: StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(type: typeof(BaseResponseWithData<LoginResponseDto>), statusCode: StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginWithGoogleAsync(GoogleLoginRequestDto googleLoginRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _authRepository.LoginWithGoogleRepositoryAsync(
                googleLoginRequestDto: googleLoginRequestDto, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(statusCode: statusCode, value: result);
            }
            //If data is not valid
            return BadRequest(new BaseResponseWithData<LoginResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }


        /// <summary>
        /// This method handles the user's registration process.
        /// </summary>
        /// <param name="registerRequestDto">The registration request data.</param>
        /// <returns>Returns a success message if registration is successful, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message if registration is successful</response>
        /// <response code="400">If the email does not exist on any email service provider's system</response>
        /// <response code="409">If the email already exists on the system</response>
        /// <response code="500">If there is an internal server error while creating the user, 
        /// adding the user to role, or sending the confirmation email</response>
        /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/auth/register
        ///     {
        ///        "Name": "Tam Hoang",
        ///        "Email": "hoangtamit20@outlook.com",
        ///        "Password": "abc@123De2"
        ///     }
        /// 
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<RegisterResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<RegisterResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<RegisterResponseDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(BaseResponseWithData<RegisterResponseDto>), StatusCodes.Status500InternalServerError)]

        [HttpPost("/api/v1/auth/register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
        {
            // check data is valid
            if (ModelState.IsValid)
            {
                // Log the start of the register process for a user with a specific email at the current time.
                _logger.LogInformation($"Register started for user: {registerRequestDto.Email} at {DateTime.UtcNow}.");
                var (statusCode, data) = await _authRepository
                    .RegisterRepositoryAsync(registerRequestDto: registerRequestDto);
                // Log the successful completion of the register process for a user with a specific email at the current time.
                if (statusCode == 200)
                    _logger.LogInformation($"Register successful for user: {registerRequestDto.Email} at {DateTime.UtcNow}.");
                return StatusCode(statusCode, data);
            }
            return BadRequest(new BaseResponseWithData<RegisterResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        /// <summary>
        /// This method handles the email confirmation process.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <param name="token">The confirmation token.</param>
        /// <returns>Returns a success message if email confirmation is successful, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message if email confirmation is successful</response>
        /// <response code="303">If the email confirmation is successful but no URL is found to navigate to the Result Page</response>
        /// <response code="400">If the id or token is null or empty</response>
        /// <response code="401">If the user does not exist on the system</response>
        /// <response code="500">If an error occurred while decoding the token or confirming the email</response>
        /// <remarks>
        /// Note : 
        /// 
        ///     This API is used to receive the result of the user’s email confirmation. You don’t need to handle this API.
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<ConfirmEmailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status303SeeOther)]
        [ProducesResponseType(typeof(BaseResponseWithData<ConfirmEmailResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<ConfirmEmailResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<ConfirmEmailResponseDto>), StatusCodes.Status500InternalServerError)]
        [HttpGet("/api/v1/auth/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var (statusCode, data) = await _authRepository.ConfirmEmailRepositoryAsync(id, token);
            if (!string.IsNullOrEmpty(data?.Data?.ConfirmEmailUrl))
            {
                var url = UrlHelper.BuildUrl(data.Data.ConfirmEmailUrl, new Dictionary<string, string>
                {
                    { "statuscode", statusCode.ToString() },
                    { "result", data.IsSuccess.ToString() },
                    { "message", data.Message },
                });
                return Redirect(url: url);
            }
            // If not exists url navigate to result page
            var messageResultError = data?.Data?.ConfirmEmailUrl == null
                && statusCode == StatusCodes.Status200OK
                ? "You have been confirm email success, but we can't navigate to Result Page"
                    : "Invalid data and not found navigate url error page";
            _logger.LogWarning(messageResultError);
            return StatusCode(StatusCodes.Status303SeeOther, messageResultError);
        }

        /*-------------------------- PROCESS REPOSITORY RESET PASSWORD ------------------------*/
        /// <summary>
        /// This method handles the process of checking the email for password reset.
        /// </summary>
        /// <param name="checkEmailResetPasswordDto">The check email reset password data transfer object.</param>
        /// <returns>Returns a success message if the email check is successful and the reset password email is sent, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message if the email check is successful and the reset password email is sent</response>
        /// <response code="400">If the email does not exist on any email service provider's system or on the system or data is not valid</response>
        /// <response code="500">If an error occurred while getting the URL to navigate to the reset password page or while sending the reset password confirmation email</response>
        /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/auth/check-email-reset-password
        ///     {
        ///        "Email": "hoangtamit20@outlook.com",
        ///     }
        /// 
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status500InternalServerError)]
        [HttpPost("/api/v1/auth/check-email-reset-password")]
        public async Task<IActionResult> CheckEmailResetPasswordAsync(CheckEmailResetPasswordDto checkEmailResetPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, data) = await _authRepository
                    .CheckEmailResetPasswordRepositoryAsync(checkEmailResetPasswordDto: checkEmailResetPasswordDto);
                return StatusCode(statusCode, data);
            }
            return BadRequest(new BaseResponseWithData<ResetPasswordResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        /// <summary>
        /// This method handles the password reset process.
        /// </summary>
        /// <param name="resetPasswordRequestDto">The password reset request data transfer object.</param>
        /// <returns>Returns a success message if password reset is successful, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message if password reset is successful</response>
        /// <response code="400">If the data is not valid</response>
        /// <response code="401">If the user does not exist on the system</response>
        /// <response code="500">If an error occurred while resetting the password</response>
        /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/auth/register
        ///     {
        ///        "id": "7994f1b3-d733-494e-b95d-00d0cdec7d45",
        ///        "token": "CfDJ8Boc%2b8li0cNMhbHXVJ%2bb1Pod01a70qfFJkKYsv7wSI4WC7T4%2fP17oTvTTLTVCk2uCKsU4UC8wQSgGrRNvdYHlfSG9Gs1A19TdopNKKX3ym4o33b3BgOeZEicvAVETlra%2fyayqpwrzRLy%2fwZm8blVgac4g5i4zGz4djJBqN2evS9aKIsKptjd1DoCKW5VCbmr1fXqDYzpet5CZ5lv4%2bs7mU9HYoH2rbe%2fT5fJM2qYEksrBktYtnnRvrRGfHGkeo9LWg%3d%3d",
        ///        "newPassword": "abc@123De2"
        ///     }
        /// 
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<ResetPasswordResponseDto>), StatusCodes.Status500InternalServerError)]
        [HttpPost("/api/v1/auth/confirm-reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, data) = await _authRepository
                    .ResetPasswordRepositoryAsync(resetPasswordRequestDto: resetPasswordRequestDto);
                return StatusCode(statusCode, data);
            }
            // If data is not valid
            return BadRequest(new BaseResponseWithData<ResetPasswordResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        /// <summary>
        /// This api is to process logout and reject accesstoken and refreshtoken of current user's device
        /// </summary>
        /// <returns>Return a success message If user valid authentication</returns>
        /// <response code="200">Returns the success message if password reset is successful</response>
        /// <response code="401">If user unauthorize</response>
        [HttpPost("/api/v1/auth/logout-current-device")]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<LogOutResponseDto>), StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> LogoutCurrentDevice()
        {
            var (statusCode, result) = await _authRepository.LogOutCurrentDeviceRepositoryAsync();
            return StatusCode(statusCode: statusCode, value: result);
        }

        /// <summary>
        /// This api is to process logout all device and reject accesstoken and refreshtoken of current user's
        /// </summary>
        /// <returns>Return a success message If user valid authentication</returns>
        /// <response code="200">Returns the success message if password reset is successful</response>
        /// <response code="401">If user unauthorize</response>
        [HttpPost("/api/v1/auth/logout-all-device")]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<LogOutResponseDto>), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> LogoutAllDevice()
        {
            var (statusCode, result) = await _authRepository.LogOutAllDeviceRepositoryAsync();
            return StatusCode(statusCode: statusCode, value: result);
        }


        /// <summary>
        /// This API process create new access token for user keep login if acess token expire
        /// </summary>
        /// <param name="refreshTokenRequestDto">Include RefreshToken: was send before</param>
        /// <returns>Return new accesstoken if refresh token request is valid or error message if refresh token is not valid</returns>
        /// <response code="200">Returns the newly created access token</response>
        /// <response code="400">If the data is not valid</response>
        /// <response code="401">If the refresh token is expired or revoked</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/auth/refresh-token
        ///     {
        ///        "refreshToken": "this string using guid"
        ///     }
        ///
        /// </remarks>
        [HttpPost("/api/v1/auth/refresh-token")]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<LogOutResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<LogOutResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<LogOutResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _authRepository
                .RefreshTokenRepositoryAsync(refreshTokenRequestDto: refreshTokenRequestDto);
                return StatusCode(statusCode: statusCode, value: result);
            }
            // If data is not valid
            return BadRequest(new BaseResponseWithData<ResetPasswordResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }
    }
}