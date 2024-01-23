using System.ComponentModel.DataAnnotations;
using System.Net;

namespace OnlineCoursePlatform.Models.Attributes
{
    public class GoogleAccessTokenAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? accessToken = value as string;
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ValidationResult("Access token is required.");
            }

            // Use Google's API to validate the access token
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}").Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ValidationResult("Invalid Google access token.");
                }
            }

            return ValidationResult.Success;
        }
    }
}