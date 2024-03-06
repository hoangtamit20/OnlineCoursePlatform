using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserCourseInteraction
    {
        [Key]
        public int Id { get; set; }
        public string? IpAddress { get; set; }
        public int ViewScore { get; set; } = 0;
        public int PurchaseScore { get; set; } = 0;
        public int FavoriteScore { get; set; } = 0;
        public int CommentScore { get; set; } = 0;
        public DateTime InteractionDate { get; set; } = DateTime.UtcNow;
        public int CourseId { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("CourseId")]
        [InverseProperty("UserCourseInteractions")]
        public virtual Course Course { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("UserCourseInteractions")]
        public virtual AppUser? User { get; set; }
    }
}