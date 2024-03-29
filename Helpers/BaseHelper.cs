using System.Web;
using DetectLanguage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Helpers.Emails.RenderHtmls;

namespace OnlineCoursePlatform.Helpers
{
    public static class BaseHelper
    {
        public static Uri CreateConfirmationUrl(string baseUrl, string userId, string token)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["id"] = userId;
            parameters["token"] = token;
            uriBuilder.Query = parameters.ToString();
            return uriBuilder.Uri;
        }

        public static async Task SendConfirmationEmailAsync(IEmailSender _emailSender, string email, Uri confirmationUrl)
        {
            var htmlMessage = RenderHtmlHelper.GetHtmlConfirmEmail(confirmationUrl, email);

            await _emailSender.SendEmailAsync(email, "Confirm Email", htmlMessage);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenFromUrl"></param>
        /// <returns></returns>
        public static BaseResponseDto DecodeTokenFromUrl(string tokenFromUrl)
        {
            try
            {
                // Check if the token is not null or empty
                if (string.IsNullOrEmpty(tokenFromUrl))
                {
                    return new BaseResponseDto() { Message = "Token is null or empty.", IsSuccess = false };
                }

                // Check if the token contains any special characters
                if (tokenFromUrl.IndexOfAny(new char[] { '+', '%', '&' }) >= 0)
                {
                    // Decode the token using HttpUtility.UrlDecode
                    string decodedToken = HttpUtility.UrlDecode(tokenFromUrl);
                    System.Console.WriteLine($"TOKEN HERE : {tokenFromUrl}");
                    return new BaseResponseDto() { Message = decodedToken, IsSuccess = true };
                }
                return new BaseResponseDto() { Message = tokenFromUrl, IsSuccess = true };
            }
            catch (Exception ex)
            {
                // Log the error message
                return new BaseResponseDto()
                {
                    Message = $"An error occurred while decoding the token: {ex.Message}",
                    IsSuccess = false
                };
            }
        }


        public static string? GetErrorsFromIdentityResult(IdentityResult identityResult)
        {
            string result = string.Empty;
            if (!identityResult.Succeeded)
            {
                foreach (var err in identityResult.Errors)
                {
                    result += err.Code + " : " + err.Description + "\n";
                }
                return result.TrimEnd('\n');
            }
            return null;
        }

        public async static Task<(string? Code, string? Name)> DetectLanguage(IFormFile file, string apiKey)
        {
            try
            {
                DetectLanguageClient client = new DetectLanguageClient(
                    apiKey: apiKey);
                // Read the content of the file
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();

                // Use the DetectLanguage client to detect the language
                string languageCode = await client.DetectCodeAsync(content);
                var languages = await client.GetLanguagesAsync();
                var languageDetected = languages
                    .Select(item => new { Code = item.code, Name = item.name })
                    .FirstOrDefault(item => item.Code == languageCode);
                return (languageDetected?.Code, languageDetected?.Name);
            }
            catch (Exception ex)
            {
                // Log the exception or rethrow it
                Console.WriteLine(ex.Message);
                throw new Exception($"An error occured while detech language from file.", ex);
            }
        }
    }
}