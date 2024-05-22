using OnlineCoursePlatform.DTOs.ChatDtos;
using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Models.UploadFileModels;

namespace OnlineCoursePlatform.Services.ChatServices
{
    public interface IChatService
    {
        Task<ChatInfoDto?> AddMessageChatAsync(AddChatRequestDto requestDto);
        Task<List<UploadChatFileModel>?> UploadChatFilesAsync(UploadChatFilesRequestDto
            requestDto);
    }
}