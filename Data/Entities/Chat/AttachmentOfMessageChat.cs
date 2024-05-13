using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    [Table("AttachmentOfMessageChat")]
    public class AttachmentOfMessageChat
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public string FileUrl { get; set; } = string.Empty;

        [StringLength(maximumLength: 255)]
        public string BlobContainerName { get; set; } = null!;

        [StringLength(maximumLength: 255)]
        public string FileName { get; set; } = null!;

        public FileType FileType { get; set; }

        public string MessageChatId { get; set; } = null!;
        [ForeignKey("MessageChatId")]
        [InverseProperty("AttachmentOfMessageChats")]
        public virtual MessageChat MessageChat { get; set; } = null!;
    }

    public enum FileType
    {
        Image = 1,
        Video = 2,
        Other = 3
    }
}