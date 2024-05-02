using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    public class UserOfGroupChat
    {
        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        [InverseProperty("UserOfGroupChats")]
        public virtual required AppUser User { get; set; }

        public required string GroupChatId { get; set; }
        [ForeignKey("GroupChatId")]
        [InverseProperty("UserOfGroupChats")]
        public virtual required GroupChat GroupChat { get; set; }

        public bool IsLeave { get; set; }

        public bool IsBanned { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public DateTime? LeaveDate { get; set; }
    }
}