using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace OnlineCoursePlatform.Data.Entities
{
    public class AppUser : IdentityUser
    {
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Address { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Picture { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [InverseProperty("User")]
        public virtual Cart Cart { get; set; } = null!;
        [InverseProperty("User")]
        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();
        [InverseProperty("User")]
        public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        [InverseProperty("User")]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        [InverseProperty("User")]
        public virtual ICollection<UserCourseInteraction> UserCourseInteractions { get; set; } = new List<UserCourseInteraction>();
    }
}