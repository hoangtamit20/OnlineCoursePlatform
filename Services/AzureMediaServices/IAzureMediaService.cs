using OnlineCoursePlatform.DTOs.AzureDtos.Request;
using OnlineCoursePlatform.DTOs.AzureDtos.Response;

namespace OnlineCoursePlatform.Services.AzureMediaServices
{
    public interface IAzureMediaService
    {
        Task<UploadAzureMediaResponseDto?> UploadMediaWithOfflinePlayReadyAndWidevineProtectionServiceAsync<T>(
            UploadAzureMediaRequestDto<T> uploadAzureMediaRequestDto, string connectionId);

        string GetToken(string keyIdentifier);
    }
}