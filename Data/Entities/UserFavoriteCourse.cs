using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserFavoriteCourse
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateAdd { get; set; } = DateTime.UtcNow;
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