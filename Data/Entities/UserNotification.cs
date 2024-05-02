using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserNotification
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public bool IsReceived { get; set; }
        public string? MessageChatId { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastRead { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("UserNotifications")]
        public virtual AppUser User { get; set; } = null!;
    }
}