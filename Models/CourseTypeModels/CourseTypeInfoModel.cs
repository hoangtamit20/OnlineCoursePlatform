using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.Models.CourseTypeModels
{
    public class CourseTypeInfoModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; } = null!;
        public DateTime CreateDate { get; set; }
    }
}