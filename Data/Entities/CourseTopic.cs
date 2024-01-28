using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class CourseTopic
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int WeeklyViews { get; set; } = 0;
        public int MonthlyViews { get; set; } = 0;
        public int TotalViews { get; set; } = 0;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public int CourseTypeId { get; set; }
        [ForeignKey("CourseTypeId")]
        [InverseProperty("CourseTopics")]
        public virtual CourseType CourseType { get; set; } = null!;
        [InverseProperty("CourseTopic")]
        public virtual ICollection<Course> Courses { get; set; } = new HashSet<Course>();
    }
}