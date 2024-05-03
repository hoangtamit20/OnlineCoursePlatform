namespace OnlineCoursePlatform.DTOs.FileUploadDtos.Request
{
    public class UploadChatFilesRequestDto
    {
        public string GroupChatId { get; set; } = null!;
        public List<IFormFile> Files { get; set; } = null!;
    }
}