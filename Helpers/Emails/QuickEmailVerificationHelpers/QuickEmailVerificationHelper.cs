using System.Web;

namespace OnlineCoursePlatform.Helpers.Emails.QuickEmailVerificationHelpers
{
    public static class QuickEmailVerificationHelper
    {
        public static async Task<EmailVerificationModel?> ValidateEmailAddressAsync(
            string emailAddress, string baseUrl, string apiKey)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string apiURL = $"{baseUrl}email={HttpUtility.UrlEncode(emailAddress)}&apikey={apiKey}";

                    return await httpClient.GetFromJsonAsync<EmailVerificationModel>(apiURL);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}