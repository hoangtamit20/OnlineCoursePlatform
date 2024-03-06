using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.UploadFileModels;

namespace OnlineCoursePlatform.Services.AzureBlobStorageServices
{
    public interface IAzureBlobStorageService
    {
        Task<List<UploadPublicFileModel>> UploadPublicFilesToAzureBlobStorageAsync(
            AppUser user,
            string? courseId,
            string? lessonId,
            IFormFile? fileThumbnail = null,
            List<IFormFile>? fileSubtitles = null);
    }
}