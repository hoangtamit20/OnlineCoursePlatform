using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Models.UploadFileModels;

namespace OnlineCoursePlatform.Services.ChatServices
{
    public interface IChatService
    {
        Task<List<UploadChatFileModel>?> UploadChatFilesAsync(UploadChatFilesRequestDto
            requestDto);
    }
}