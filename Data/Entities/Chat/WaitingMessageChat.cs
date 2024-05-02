using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    [Table(name: "WaitingMessageChat")]
    public class WaitingMessageChat
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public required string MessageChatId { get; set; }
        [ForeignKey("MessageChatId")]
        [InverseProperty("WaitingMessageChats")]
        public virtual required MessageChat MessageChat { get; set; }

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        [InverseProperty("WaitingMessageChats")]
        public virtual required AppUser User { get; set; }
    }
}