using OnlineCoursePlatform.Data.Entities.Chat;
using OnlineCoursePlatform.Models.UploadFileModels;

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
        public string Id { get; set; } = null!;
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
        public string? GroupChatId { get; set; }
        public string? UserId { get; set; }
    }

    public class GroupChatModel
    {
        public string? GroupChatId { get; set; }
        public List<string> UserOfGroupChats { get; set; } = new();
    }

    // public class UploadFilesChatRequestDto
    // {
    //     public string GroupChatId { get; set; } = string.Empty;
    //     public List<IFormFile> Files { get; set; } = new();
    // }

    public class UploadFilesChatResponseDto
    {
        public List<ChatFileInfo> ChatFileInfos { get; set; } = new();
    }

    public class AddChatRequestDto
    {
        public string GroupChatId { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public List<UploadChatFileModel> UploadChatFileModels { get; set; } = new();
    }
}