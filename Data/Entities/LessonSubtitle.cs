using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class LessonSubtitle
    {
        [Key]
        public int Id { get; set; }
        public string Language { get; set; } = null!;
        public string UrlSubtitle { get; set; } = null!;
        public DateTime DateAdd { get; set; } = DateTime.UtcNow;
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        [InverseProperty("LessonSubtitles")]
        public virtual Lesson Lesson { get; set; } = null!;
    }
}