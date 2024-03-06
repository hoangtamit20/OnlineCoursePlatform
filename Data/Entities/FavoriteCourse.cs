using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserFavoriteCourse
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("UserFavoriteCourses")]
        public virtual AppUser User { get; set; } = null!;
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        [InverseProperty("UserFavoriteCourses")]
        public virtual Course Course { get; set; } = null!;

    }
}