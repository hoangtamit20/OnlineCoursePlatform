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
        public required string BlobContainerName { get; set; }

        [StringLength(maximumLength: 255)]
        public required string FileName { get; set; }

        public FileType FileType { get; set; }

        public required string MessageChatId { get; set; }
        [ForeignKey("MessageChatId")]
        [InverseProperty("AttachmentOfMessageChats")]
        public virtual required MessageChat MessageChat { get; set; }
    }

    public enum FileType
    {
        Image = 1,
        Video = 2,
        Other = 3
    }
}