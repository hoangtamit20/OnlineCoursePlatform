using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;
        [StringLength(maximumLength: 100)]
        public string? BlobContainerName { get; set; }
        [StringLength(maximumLength: 250)]
        public string? FileThumbnailName { get; set; }
        [StringLength(1000)]
        public string? Thumbnail { get; set; }
        public bool IsPublic { get; set; } = true;
        // public bool IsDelete { get; set; } = false;
        public bool? IsFree { get; set; }
        [StringLength(1000)]
        public string? CourseDescription { get; set; }
        public int MonthlySales { get; set; } = 0;
        public int TotalSales { get; set; } = 0;
        public int TotalViews { get; set; } = 0;
        public int WeeklyViews { get; set; } = 0;
        public int MonthlyViews { get; set; } = 0;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public int CourseTopicId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("Courses")]
        public virtual AppUser User { get; set; } = null!;
        [ForeignKey("CourseTopicId")]
        [InverseProperty("Courses")]
        public virtual CourseTopic CourseTopic { get; set; } = null!;
        [InverseProperty("Course")]
        public ICollection<UserCourseInteraction> UserCourseInteractions { get; set; } = new List<UserCourseInteraction>();
        [InverseProperty("Course")]
        public virtual ICollection<Lesson> Lessons { get; set; } = new HashSet<Lesson>();
        [InverseProperty("Course")]
        public virtual ICollection<Cart> Carts { get; set; } = new HashSet<Cart>();
        [InverseProperty("Course")]
        public virtual ICollection<CourseSubtitle> CourseSubtitles { get; set; } = new List<CourseSubtitle>();
        [InverseProperty("Course")]
        public virtual ICollection<CourseUrlStreaming> CourseUrlStreamings { get; set; } = new List<CourseUrlStreaming>();
    }
}