using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Helpers.UrlHelpers;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelperController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<HelperController> _logger;
        public HelperController(
            OnlineCoursePlatformDbContext dbContext, 
            ILogger<HelperController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlResetPassword"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/helper/set-url-reset-password")]
        public async Task<IActionResult> SeedUrlResetPassword(string urlResetPassword)
        {
            var helper = await _dbContext.UrlHelperEntities.FirstOrDefaultAsync();
            try{
                if (helper is not null)
                {
                    helper.ResetPasswordUrl = urlResetPassword;
                    _dbContext.Entry<UrlHelperEntity>(helper).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Update url to navigate after user want to reset password success");
                }
                else
                {
                    helper = new UrlHelperEntity()
                    {
                        ResetPasswordUrl = urlResetPassword
                    };
                    _dbContext.UrlHelperEntities.Add(helper);
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Create url to navigate after user want to reset password success");
                }
            }catch(Exception ex)
            {
                _logger.LogWarning(ex, $"{DateTime.UtcNow}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlConfirmEmail"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/helper/set-url-confirm-email")]
        public async Task<IActionResult> SeedUrlConfirmEmail(string urlConfirmEmail)
        {
            var helper = await _dbContext.UrlHelperEntities.FirstOrDefaultAsync();
            try{
                if (helper is not null)
                {
                    helper.ConfirmEmailUrl = urlConfirmEmail;
                    _dbContext.Entry<UrlHelperEntity>(helper).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Update url to navigate after user want to confirm email success");
                }
                else
                {
                    helper = new UrlHelperEntity()
                    {
                        ConfirmEmailUrl = urlConfirmEmail
                    };
                    _dbContext.UrlHelperEntities.Add(helper);
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Create url to navigate after user want to confirm email success");
                }
            }catch(Exception ex)
            {
                _logger.LogWarning(ex, $"{DateTime.UtcNow}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlConfirmEmail"></param>
        /// <returns></returns>
        /// <remarks>
        /// Example:
        /// 
        ///     Online :  https://app-tamhoang-web-eastas-dev-001.azurewebsites.net/api/v1/auth/confirm-email
        ///     Offline : https://localhost:7209/api/v1/auth/confirm-email
        /// 
        /// </remarks>
        [HttpPost("/api/v1/helper/set-url-confirm-email-from-email-client")]
        public async Task<IActionResult> SeedUrlConfirmEmailFromClien(string urlConfirmEmail)
        {
            var helper = await _dbContext.UrlHelperEntities.FirstOrDefaultAsync();
            try{
                if (helper is not null)
                {
                    helper.ConfirmEmailFromClientUrl = urlConfirmEmail;
                    _dbContext.Entry<UrlHelperEntity>(helper).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Update url to navigate after user want to confirm email success");
                }
                else
                {
                    helper = new UrlHelperEntity()
                    {
                        ConfirmEmailFromClientUrl = urlConfirmEmail
                    };
                    _dbContext.UrlHelperEntities.Add(helper);
                    await _dbContext.SaveChangesAsync();
                    return Ok($"Create url to navigate after user want to confirm email success");
                }
            }catch(Exception ex)
            {
                _logger.LogWarning(ex, $"{DateTime.UtcNow}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


        /// <summary>
        /// test auth
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = RolesConstant.Learner)]
        public string TestAuth() => "dalhadliahd";

        [HttpGet("TestAuthAc")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesConstant.Learner)]
        public string TestAusth() => "dalhadliahd";
    }
}