using OnlineCoursePlatform.Models.AzureMediaServices;

namespace OnlineCoursePlatform.DTOs.AzureDtos.Response
{
    public class UploadAzureMediaResponseDto
    {
        public string KeyIdentifier { get; set; } = null!;
        public PathStreamingModel PathStreamingModel { get; set; } = new PathStreamingModel();
    }
    // public List<(string quality, string urlDownload)> UrlDownloads { get; set; } = new();

}