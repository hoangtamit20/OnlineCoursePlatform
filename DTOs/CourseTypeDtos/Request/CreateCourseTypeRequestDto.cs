using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.CourseTypeDtos.Request
{
    public class CreateCourseTypeRequestDto
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; } = null!;
    }
}