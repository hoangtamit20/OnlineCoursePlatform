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

        public string GroupChatId { get; set; } = null!;
        [ForeignKey("GroupChatId")] 
        [InverseProperty("MessageChats")]
        public virtual GroupChat GroupChat { get; set; } = null!;

        public string SenderId { get; set; } = null!;
        [ForeignKey("SenderId")]
        [InverseProperty("MessageChats")]
        public virtual AppUser Sender { get; set; } = null!;

        [InverseProperty("MessageChat")]
        public virtual ICollection<AttachmentOfMessageChat> AttachmentOfMessageChats { get; set; } = new List<AttachmentOfMessageChat>();

        [InverseProperty("MessageChat")]
        public virtual ICollection<WaitingMessageChat> WaitingMessageChats { get; set; } = new List<WaitingMessageChat>();
    }
}