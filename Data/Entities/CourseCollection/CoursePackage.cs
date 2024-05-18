using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.CourseCollection
{
    public class PackageCourse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public PackageCourseType Type { get; set; }
        public string Name { get; set; } = null!;
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        [InverseProperty("PackageCourses")]
        public virtual Course Course { get; set; } = null!;
    }

    public enum PackageCourseType
    {
        Month = 1,
        Year = 2,
        Unlimited = 3
    }
}