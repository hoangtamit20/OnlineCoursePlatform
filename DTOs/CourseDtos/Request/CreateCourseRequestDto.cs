using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Models.Attributes;

namespace OnlineCoursePlatform.DTOs.CourseDtos.Request
{
    public class CreateCourseRequestDto
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;
        [StringLength(maximumLength: 1000, ErrorMessage = "{0} must be less {1} characters.")]
        public string? CourseDescription { get; set; }
        [Required(ErrorMessage = "{0} is required.")]
        public int CourseTopicId { get; set; }
        [NotMapped]
        [AllowedExtensions(extensions: new string[] { ".srt", ".vtt", ".sbv" })]
        public List<IFormFile>? SubtitleFileUploads { get; set; }
        [NotMapped]
        [AllowedExtensions(extensions: new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile? ThumbnailFileUpload { get; set; }
        [NotMapped]
        [AllowedExtensions(extensions: new string[] { ".mp4", ".avi", ".mov" })]
        public IFormFile? VideoFileUpload { get; set; }
    }
}