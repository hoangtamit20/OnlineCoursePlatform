using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class CourseSubtitle
    {
        [Key]
        public int Id { get; set; }
        [StringLength(maximumLength: 50)]
        public string Language { get; set; } = null!;
        [StringLength(maximumLength: 100)]
        public string ContainerName { get; set; } = null!;
        [StringLength(maximumLength: 250)]
        public string FileName { get; set; } = null!;
        [StringLength(maximumLength: 1000)]
        public string UrlSubtitle { get; set; } = null!;
        public DateTime DateAdd { get; set; } = DateTime.UtcNow;
        public virtual int CourseId { get; set; }
        [ForeignKey("CourseId")]
        [InverseProperty("CourseSubtitles")]
        public virtual Course Course { get; set; } = null!;
    }
}