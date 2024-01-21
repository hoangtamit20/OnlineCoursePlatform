using System.Web;
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
            var htmlMessage = RenderHtmlHelper.GetHtmlConfirmEmail(confirmationUrl);

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
                    return new BaseResponseDto() { Message = decodedToken, IsSuccess = true };
                }
                return new BaseResponseDto() { Message = tokenFromUrl, IsSuccess = true };
            }
            catch (Exception ex)
            {
                // Log the error message
                return new BaseResponseDto() { 
                    Message = $"An error occurred while decoding the token: {ex.Message}", 
                    IsSuccess = false };
            }
        }
    }
}