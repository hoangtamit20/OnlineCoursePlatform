using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    [Table("CourseTypes")]
    public class CourseType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        [InverseProperty("CourseType")]
        public virtual ICollection<CourseTopic> CourseTopics { get; set; } = new List<CourseTopic>();

    }
}