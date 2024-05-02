using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    [Table("MessageChat")]
    public class MessageChat
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public string MessageText { get; set; } = string.Empty;

        public DateTime SendDate { get; private set; } = DateTime.UtcNow;

        public bool IsIncludedFile { get; set; }

        public required string GroupChatId { get; set; }
        [ForeignKey("GroupChatId")] 
        [InverseProperty("MessageChats")]
        public virtual required GroupChat GroupChat { get; set; }

        public required string SenderId { get; set; }
        [ForeignKey("SenderId")]
        [InverseProperty("MessageChats")]
        public virtual required AppUser Sender { get; set; }

        [InverseProperty("MessageChat")]
        public virtual ICollection<AttachmentOfMessageChat> AttachmentOfMessageChats { get; set; } = new List<AttachmentOfMessageChat>();

        [InverseProperty("MessageChat")]
        public virtual ICollection<WaitingMessageChat> WaitingMessageChats { get; set; } = new List<WaitingMessageChat>();
    }
}