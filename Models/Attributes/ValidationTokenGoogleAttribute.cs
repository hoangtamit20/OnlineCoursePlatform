using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth;
using Newtonsoft.Json;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.DTOs.AuthDtos.Request;
using OnlineCoursePlatform.Services.AuthServices;

namespace OnlineCoursePlatform.Models.Attributes
{
    public class ValidationTokenGoogleAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var googleToken = value as string;
            var _configuration = (IConfiguration)validationContext.GetService(typeof(IConfiguration))!;
            var _logger = (ILogger<RegisterService>)validationContext.GetService(typeof(ILogger<RegisterService>))!;

            if (string.IsNullOrEmpty(googleToken))
            {
                return new ValidationResult("Google token is required.");
            }

            var clientId = _configuration[AppSettingsConfig.GOOGLE_CLIENTID_WEB]!;
            var clientIdMobile = _configuration[AppSettingsConfig.GOOGLE_CLIENTID_MOBILE]!;

            try
            {
                // Try to get payload from idtoken
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { clientId, clientIdMobile},
                };

                var payload = GoogleJsonWebSignature.ValidateAsync(googleToken, settings).Result;
                return ValidationResult.Success;
            }
            catch (Exception ex)
            {
                // Try to get token info from access token if ID token fails
                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = client.GetAsync($"{GoogleApiUrlConstant.UrlTokenInfo}{googleToken}").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        var tokenInfo = JsonConvert.DeserializeObject<TokenInfoRequestDto>(content);

                        if (
                            tokenInfo?.audience == clientId
                            || tokenInfo?.audience == clientIdMobile)
                        {
                            return ValidationResult.Success;
                        }
                        else
                        {
                            return new ValidationResult("Token google not valid.");
                        }
                    }
                    else
                    {
                        return new ValidationResult("Token google not valid.");
                    }
                }
                catch (Exception ex1)
                {
                    _logger.LogError($"Internal Server Error : Failed to get user info from Google Access token.\nTrace Log : {ex.Message}{ex1.Message}");
                    return new ValidationResult("Token google not valid.");
                }
            }
        }
    }
}