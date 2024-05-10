using OnlineCoursePlatform.Data.Entities.Chat;

namespace OnlineCoursePlatform.DTOs.ChatDtos
{

    public class ChatResponseDto
    {
        public GroupChatInfoDto GroupChatInfoDto { get; set; } = null!;
        public List<ChatInfoDto> ChatInfoDtos { get; set; } = new List<ChatInfoDto>();
    }

    public class ChatInfoDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public bool IsCurrent { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public string MessageChatId { get; set; } = string.Empty;
        public DateTime SendDate { get; set; }
        public List<ChatFileInfo> ChatFileInfos { get; set; } = new List<ChatFileInfo>();
    }

    public class ChatFileInfo
    {
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public FileType FileType { get; set; }
    }

    public class GroupChatInfoDto
    {
        public string GroupChatId { get; set; } = string.Empty;
        public string GroupChatName { get; set; } = string.Empty;
        public string? AdminId { get; set; }
    }


    public class UserIdModel
    {
        public string? UserId { get; set; }
    }

    public class GroupChatModel
    {
        public string? GroupChatId { get; set; }
        public List<string> UserOfGroupChats { get; set; } = new();
    }
}