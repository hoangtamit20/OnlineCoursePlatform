using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    [Table(name: "GroupChat")]
    public class GroupChat
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [StringLength(maximumLength: 150)]
        public required string Name { get; set; }

        public GroupChatType Type { get; set; }

        public DateTime CreateDate { get; private set; } = DateTime.UtcNow;

        public string? AdminId { get; set; }

        [ForeignKey("AdminId")]
        [InverseProperty("AdminOfGroup")]
        public virtual AppUser? Admin { get; set; }

        [InverseProperty("GroupChat")]
        public ICollection<UserOfGroupChat> UserOfGroupChats { get; set; } = new List<UserOfGroupChat>();

        [InverseProperty("GroupChat")]
        public ICollection<MessageChat> MessageChats { get; set; } = new List<MessageChat>();
    }

    public enum GroupChatType
    {
        Conversation = 1,
        Group = 2
    }
}