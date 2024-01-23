using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth;

namespace OnlineCoursePlatform.Models.Attributes
{
    public class ValidateGoogleIdTokenAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var idToken = value as string;

            if (string.IsNullOrEmpty(idToken))
            {
                return new ValidationResult("ID token is required.");
            }

            try
            {
                var configuration = (IConfiguration)validationContext.GetService(typeof(IConfiguration))!;
                var clientId = configuration["Google:ClientId"]!;

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { clientId },
                };

                var payload = GoogleJsonWebSignature.ValidateAsync(idToken, settings).Result;

                return ValidationResult.Success;
            }
            catch (Exception)
            {
                return new ValidationResult("Invalid Google ID token.");
            }
        }
    }
}