using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class LessonUrlStreaming
    {
        [Key]
        public int Id { get; set; }
        [StringLength(maximumLength: 1000)]
        public string StreamUrlDashCsf { get; set; } = null!;
        [StringLength(maximumLength: 1000)]
        public string StreamUrlDashCmaf { get; set; } = null!;
        [StringLength(maximumLength: 1000)]
        public string StreamUrlSmooth { get; set; } = null!;
        [StringLength(maximumLength: 250)]
        public string? AssetName { get; set; }
        [StringLength(maximumLength: 250)]
        public string? LocatorName { get; set; }
        [StringLength(maximumLength: 500)]
        public string? SigningTokenKey { get; set; }
        [StringLength(maximumLength: 1000)]
        public string? DownloadUrl { get; set; }
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        [InverseProperty("LessonUrlStreamings")]
        public virtual Lesson Lesson { get; set; } = null!;
    }
}