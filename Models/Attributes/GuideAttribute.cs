using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.Models.Attributes
{
    public class GuidAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var guidString = value as string;
        Guid guid;

        // Kiểm tra xem chuỗi có phải là Guid hợp lệ hay không
        if (!Guid.TryParse(guidString, out guid))
        {
            return new ValidationResult("Invalid Guid format");
        }

        return ValidationResult.Success;
    }
}

}