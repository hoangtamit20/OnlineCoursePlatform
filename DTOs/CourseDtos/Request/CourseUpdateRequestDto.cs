using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Models.Attributes;

namespace OnlineCoursePlatform.DTOs.CourseDtos.Request
{
    public class CourseUpdateRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public bool IsPublic { get; set; }
        public string? CourseDescription { get; set; }
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