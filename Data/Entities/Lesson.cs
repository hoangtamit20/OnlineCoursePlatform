using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!;

        [Required]
        public int LessonIndex { get; set; }

        [StringLength(1000)]
        public string? Thumbnail { get; set; }

        public DateTime? DateRelease { get; set; }

        public bool? IsPublic { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long FileSize { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(19,0)")]
        public decimal UploadCost { get; set; } = 0;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        [InverseProperty("Lessons")]
        public virtual Course Course { get; set; } = null!;
        [InverseProperty("Lesson")]
        public virtual ICollection<LessonSubtitle> LessonSubtitles { get; set; } = new List<LessonSubtitle>();
        [InverseProperty("Lesson")]
        public virtual ICollection<LessonUrlStreaming> LessonUrlStreamings { get; set; } = new List<LessonUrlStreaming>();

    }

}